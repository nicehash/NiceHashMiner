using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.Globalization;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.IO;
using NiceHashMinerLegacy.Common;

namespace T_Rex
{
    public class T_Rex : MinerBase
    {
        private string _devices;

        private string _extraLaunchParameters = "";

        private int _apiPort;

        private readonly string _uuid;

        private AlgorithmType _algorithmType;

        private readonly HttpClient _httpClient = new HttpClient();

        public T_Rex(string uuid)
        {
            _uuid = uuid;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Lyra2Z: return "lyra2z";
                case AlgorithmType.Skunk: return "skunk";
                case AlgorithmType.X16R: return "x16r";
                default: return "";
            }
        }

        private double DevFee
        {
            get
            {
                return 1.0;
            }
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(summaryApiResult);
            
                var gpuDevices = _miningPairs.Select(pair => pair.device);
                var perDeviceSpeedInfo = new List<(string uuid, IReadOnlyList<(AlgorithmType, double)>)>();
                var perDevicePowerInfo = new List<(string, int)>();
                var totalSpeed = summary.hashrate;
                var totalPowerUsage = 0.0;

                foreach (var gpuDevice in gpuDevices)
                {
                    var currentStats = summary.gpus.Where(devStats => devStats.gpu_id == gpuDevice.ID).FirstOrDefault();
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add((gpuDevice.UUID, new List<(AlgorithmType, double)>() { (_algorithmType, currentStats.hashrate) }));
                    totalPowerUsage += currentStats.power;
                    perDevicePowerInfo.Add((gpuDevice.UUID, currentStats.hashrate));
                }

                var total = new List<(AlgorithmType, double)>();
                total.Add((_algorithmType, totalSpeed));
                ad.AlgorithmSpeedsTotal = total;
                ad.PowerUsageTotal = Convert.ToInt32(totalPowerUsage);
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;

            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
             }

            return ad;
        }

        public override async Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var benchmarkTime = 20; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 20;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    break;
            }

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} {_devices} --benchmark --time-limit {benchmarkTime}";
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            var benchHashes = 0d;
            var counter = 0;
            var benchHashResult = 0d;

            bp.CheckData = (string data) =>
            {
                var (hashrate, found) = data.TryGetHashrateAfter("Total");

                if (data.Contains("Time limit is reached."))
                    return (benchHashResult, true);

                if (!found) return (benchHashResult, false);

                benchHashes += hashrate;
                counter++;

                benchHashResult = (benchHashes / counter) * (1 - DevFee * 0.01);
                
                return (benchHashResult, false);
            };
            
            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }


        protected override (string, string) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "t-rex.exe");
            var binCwd = pluginRootBins;
            return (binPath, binCwd);
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmType, ok) = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");
            // all good continue on

            // init command line params parts
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.device.ID.CompareTo(b.device.ID));
            _devices = string.Join(",", orderedMiningPairs.Select(p => p.device.ID));
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
            // API port function might be blocking
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange(); // use the default range
            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url {url} --user {_username} --api-bind-http 127.0.0.1:{_apiPort} --devices {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}

