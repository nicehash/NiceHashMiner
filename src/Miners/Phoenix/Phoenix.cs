using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.ClaymoreCommon;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Phoenix
{
    public class Phoenix : MinerBase, IBeforeStartMining
    {
        private const double DevFee = 0.65;

        private int _apiPort;
        private string _devices;
        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public Phoenix(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedDeviceIds = mappedIDs;
        }

        public string CreateCommandLine(string username)
        {
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var deviceType = _miningPairs.FirstOrDefault().Device.DeviceType == DeviceType.AMD ? " -amd" : " -nvidia";
            var cmd = $"-pool {urlWithPort} -wal {username} -proto 4 {deviceType} -gpus {_devices} -wdog 0 -gbase 0 {_extraLaunchParameters}";

            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmType };
            // multiply dagger API data 
            var ad = await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, 0.0, algorithmTypes);
            if (ad.AlgorithmSpeedsTotal != null)
            {
                // speed is in khs
                ad.AlgorithmSpeedsTotal = ad.AlgorithmSpeedsTotal.Select((ts) => (ts.type, ts.speed * 1000)).ToList();
            }
            if (ad.AlgorithmSpeedsPerDevice != null)
            {
                // speed is in khs
                ad.AlgorithmSpeedsPerDevice = ad.AlgorithmSpeedsPerDevice.Select(pair => new KeyValuePair<string, IReadOnlyList<(AlgorithmType type, double speed)>>(pair.Key, pair.Value.Select((ts) => (ts.type, ts.speed * 1000)).ToList())).ToDictionary(x => x.Key, x => x.Value);
            }
            return ad;
        }

        //public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        //{
        //    var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 60, 90, 180 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

        //    var deviceType = _miningPairs.FirstOrDefault().Device.DeviceType == DeviceType.AMD ? "-amd" : "-nvidia";

        //    // local benchmark
        //    // TODO hardcoded epoch
        //    var commandLine = $"-gpus {_devices} -gbase 0 -bench 200 -wdog 0 {deviceType} {_extraLaunchParameters}";
        //    var binPathBinCwdPair = GetBinAndCwdPaths();
        //    var binPath = binPathBinCwdPair.Item1;
        //    var binCwd = binPathBinCwdPair.Item2;
        //    Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
        //    var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

        //    var benchHashes = 0d;
        //    var benchIters = 0;
        //    var benchHashResult = 0d;
        //    var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

        //    bp.CheckData = (string data) =>
        //    {
        //        var hashrateFoundPairFirst = data.TryGetHashrateAfter("Eth speed:");
        //        var hashrateFirst = hashrateFoundPairFirst.Item1;
        //        var foundFirst = hashrateFoundPairFirst.Item2;

        //        if (foundFirst)
        //        {
        //            benchHashes += hashrateFirst;
        //            benchIters++;

        //            benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);
        //        }

                
        //        return new BenchmarkResult
        //        {
        //            AlgorithmTypeSpeeds = new List<(AlgorithmType type, double speed)> { (_algorithmType, benchHashResult) }
        //        };
        //    };

        //    var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
        //    var benchmarkWait = TimeSpan.FromMilliseconds(500);
        //    var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
        //    return await t;
        //}

        private static HashSet<string> _deleteConfigs = new HashSet<string> { "config.txt", "dpools.txt", "epools.txt" };
        private static bool IsDeleteConfigFile(string file)
        {
            foreach(var conf in _deleteConfigs)
            {
                if (file.Contains(conf)) return true;
            }
            return false;
        }

        public void BeforeStartMining()
        {
            var binCwd = GetBinAndCwdPaths().Item2;
            var txtFiles = Directory.GetFiles(binCwd, "*.txt", SearchOption.AllDirectories)
                .Where(file => IsDeleteConfigFile(file))
                .ToArray();
            foreach(var deleteFile in txtFiles)
            {
                try
                {
                    File.Delete(deleteFile);
                }
                catch (Exception e)
                {
                    Logger.Error(_logGroup, $"BeforeStartMining error while deleting file '{deleteFile}': {e.Message}");
                }
            }
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
            _devices = string.Join(",", mappedDevIDs);
        }

        protected override string MiningCreateCommandLine()
        {
            _apiPort = GetAvaliablePort();
            return CreateCommandLine(_username) + $" -mport 127.0.0.1:-{_apiPort}";
        }
    }
}
