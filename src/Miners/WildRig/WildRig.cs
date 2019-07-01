using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WildRig
{
    public class WildRig : MinerBase
    {
        private int _apiPort;
        private string _extraLaunchParameters = "";
        private AlgorithmType _algorithmType;
        private readonly HttpClient _http = new HttpClient();
        private string _devices = "";
        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.Lyra2REv3:
                        return "lyra2v3";
                    case AlgorithmType.X16R:
                        return "x16r";
                    default:
                        return "";
                }
            }
        }

        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.Lyra2REv3:
                    case AlgorithmType.X16R:
                    default:
                        return 2.0;
                }
            }
        }

        public WildRig(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedIDs = mappedIDs;
        }


        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                var hashrate = summary.hashrate;
                if (hashrate != null) {
                    for (int i = 0; i < gpus.Count(); i++)
                    {
                        var deviceSpeed = hashrate.threads.ElementAtOrDefault(i).FirstOrDefault();
                        totalSpeed += deviceSpeed;
                        perDeviceSpeedInfo.Add(gpus.ElementAt(i)?.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, deviceSpeed * (1 - DevFee * 0.01)) });
                    }
                }

                ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            //only 60s mark is available
            var benchmarkTime = 60; // in seconds
            var commandLine = $"-a {AlgoName} --benchmark -d {_devices} --multiple-instance {_extraLaunchParameters}";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashResult = 0d;

            bp.CheckData = (string data) =>
            {

                var hashrateFoundPair = BenchmarkHelpers.TryGetHashrateAfter(data, "60s:");
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;

                if (found)
                {
                    benchHashResult = hashrate * (1 - DevFee * 0.01);
                }

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = found
                };
            };

            // always add 10second extra
            var benchmarkTimeout = TimeSpan.FromSeconds(10 + benchmarkTime);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Info(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }

            // Order pairs and parse ELP
            var orderedMiningPairs = _miningPairs.ToList();
            _devices = string.Join(",", _miningPairs.Select(p => _mappedIDs[p.Device.UUID]));
            if (MinerOptionsPackage != null)
            {
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            return $"-a {AlgoName} -o {url} -u {username} --api-port={_apiPort} -d {_devices} --multiple-instance {_extraLaunchParameters}";
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "wildrig.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }
    }
}
