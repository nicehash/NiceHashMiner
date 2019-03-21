using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;
using System.IO;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;

namespace BMiner
{
    public class BMiner : MinerBase
    {
        private string _devices;
        private string _extraLaunchParameters = "";
        private int _apiPort;
        private readonly string _uuid;
        private AlgorithmType _algorithmType;
        private readonly HttpClient _http = new HttpClient();

        public BMiner(string uuid)
        {
            _uuid = uuid;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.ZHash: return "zhash";
                case AlgorithmType.Beam: return "beam";
                case AlgorithmType.GrinCuckaroo29: return "cuckaroo29";
                case AlgorithmType.GrinCuckatoo31: return "cuckatoo31";
                default: return "";
            }
        }

        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.ZHash:
                    case AlgorithmType.Beam:
                    case AlgorithmType.GrinCuckaroo29:
                    case AlgorithmType.GrinCuckatoo31:
                    default: return 2.0;
                }
            }
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/api/status");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.device);
                var perDeviceSpeedInfo = new List<(string uuid, IReadOnlyList<(AlgorithmType, double)>)>();
                var perDevicePowerInfo = new List<(string, int)>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                foreach(var gpu in gpus)
                {
                    if (summary.miners == null) continue;

                    totalSpeed += summary.miners[$"{gpu.ID}"].solver.solution_rate;
                    perDeviceSpeedInfo.Add((gpu.UUID, new List<(AlgorithmType, double)>() { (_algorithmType, summary.miners[$"{gpu.ID}"].solver.solution_rate) }));
                    totalPowerUsage += summary.miners[$"{gpu.ID}"].device.power;
                    perDevicePowerInfo.Add((gpu.UUID, summary.miners[$"{gpu.ID}"].device.power));
                }

                api.AlgorithmSpeedsTotal = new List<(AlgorithmType, double)> { (_algorithmType, totalSpeed) };
                api.PowerUsageTotal = totalPowerUsage;
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: {e}");
            }

            return api;
        }

        public async override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = 30; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 30;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    break;
            }

            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"-uri {algo}://{_username}@{url}:{port} {_devices} -watchdog=false {_extraLaunchParameters}";
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var (hashrate, found) = data.TryGetHashrateAfter("Total");

                if (!found) return (benchHashResult, false);

                benchHashes += hashrate;
                benchIters++;

                benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);

                return (benchHashResult, benchIters >= targetBenchIters);
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override (string binPath, string binCwd) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "bminer.exe");
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
            var deviceIDs = _miningPairs.Select(p =>
            {
                var device = p.device;
                var prefix = device.DeviceType == DeviceType.AMD ? "amd:" : "";
                return prefix + device.ID;
            }).OrderBy(id => id);
            _devices = $"-devices {string.Join(",", deviceIDs)}";

            if(MinerOptionsPackage != null)
            {
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
            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];

            var algo = AlgorithmName(_algorithmType);
            var commandLine = $"-uri {algo}://{_username}@{url}:{port} -api 127.0.0.1:{_apiPort} {_devices} -watchdog=false {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
