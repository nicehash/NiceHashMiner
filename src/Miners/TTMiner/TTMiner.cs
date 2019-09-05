using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Enums;
using MinerPluginToolkitV1.ClaymoreCommon;
using MinerPluginToolkitV1.Configs;

namespace TTMiner
{
    // TODO get ordered API data
    public class TTMiner : MinerBase, IAfterStartMining
    {
        private int _apiPort;
        private string _devices;
        private double DevFee = 1d;
        // TODO figure out how to fix API workaround without this started time
        private DateTime _started;

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        private string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.MTP:
                        return "mtp";
                    case AlgorithmType.Lyra2REv3:
                        return "LYRA2V3";
                    default:
                        return "";
                }
            }
        }

        public TTMiner(string uuid, Dictionary<string, int> mappedDeviceIds) : base(uuid)
        {
            _mappedDeviceIds = mappedDeviceIds;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 20, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;
            var targetBenchIters = 2; //Math.Max(1, (int)Math.Floor(benchTime / 20d));

            bp.CheckData = (data) =>
            {
                if (data.Contains("GPU[") && data.Contains("last:"))
                {
                    var hashrateFoundPair = data.ToLower().TryGetHashrateAfter("]:");
                    var hashrate = hashrateFoundPair.Item1;
                    var found = hashrateFoundPair.Item2;

                    if (found && hashrate > 0)
                    {
                        benchHashes += hashrate;
                        benchIters++;
                        benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);
                    }
                }
                
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = benchIters >= targetBenchIters
                };
            };

            var timeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, timeout, benchWait, stop);
            return await t;
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "TT-Miner.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"-a {AlgoName} -url {url} -u {username} -d {_devices} --api-bind 127.0.0.1:{_apiPort} {_extraLaunchParameters}";
            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            var elapsedSeconds = DateTime.Now.Subtract(_started).Seconds;
            if (elapsedSeconds < 15)
            {
                return api;
            }

            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmType };
            return await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, 0.0, algorithmTypes);
        }

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by _mappedDeviceIds
            pairsList.Sort((a, b) => _mappedDeviceIds[a.Device.UUID].CompareTo(_mappedDeviceIds[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            var mappedDevIDs = _miningPairs.Select(p => _mappedDeviceIds[p.Device.UUID]);
            _devices = string.Join(" ", mappedDevIDs);
        }

        public void AfterStartMining()
        {
            _started = DateTime.Now;
        }
    }
}
