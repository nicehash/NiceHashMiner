using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace TRex
{
    public class TRex : MinerBase
    {
        private string _devices;

        private int _apiPort;

        private readonly HttpClient _httpClient = new HttpClient();

        public TRex(string uuid) : base(uuid)
        {}

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(summaryApiResult);
            
                var gpuDevices = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = summary.hashrate;
                var totalPowerUsage = 0.0;

                foreach (var gpuDevice in gpuDevices)
                {
                    var currentStats = summary.gpus.Where(devStats => devStats.gpu_id == gpuDevice.ID).FirstOrDefault();
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add(gpuDevice.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentStats.hashrate * (1 - DevFee * 0.01)) });
                    var kPower = currentStats.power * 1000;
                    totalPowerUsage += kPower;
                    perDevicePowerInfo.Add(gpuDevice.UUID, kPower);
                }
                ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                ad.PowerUsageTotal = Convert.ToInt32(totalPowerUsage);
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;

            }
            catch (Exception e) {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 20, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            var algo = AlgorithmName(_algorithmType);               
            var commandLine = $"--algo {algo} --devices {_devices} --benchmark --time-limit {benchmarkTime} {_extraLaunchParameters}";            
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var counter = 0;
            var benchHashResult = 0d;

            bp.CheckData = (string data) =>
            {
                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, "Total");
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;

                if (!found) return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) }, Success = false };

                benchHashes += hashrate;
                counter++;

                benchHashResult = (benchHashes / counter) * (1 - DevFee * 0.01);

                if (_algorithmType == AlgorithmType.X16R)
                {
                    // Quick adjustment, x16r speeds are overestimated by around 3.5
                    benchHashResult /= 3.5;
                }

                return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) }, Success = false };
            };
            
            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override void Init()
        {
            _devices = string.Join(",", _miningPairs.Select(p => p.Device.ID));
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url {url} --user {_username} --api-bind-http 127.0.0.1:{_apiPort} --api-bind-telnet 0 --devices {_devices} {_extraLaunchParameters} --no-watchdog";
            return commandLine;
        }
    }
}

