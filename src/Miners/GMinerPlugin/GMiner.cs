using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Device;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.IO;
using NiceHashMinerLegacy.Common;

namespace GMinerPlugin
{
    // NOTE: GMiner will NOT run if the VS debugger is attached to NHML. 
    // Detach the debugger to use GMiner.

    // benchmark is 
    public class GMiner : MinerBase
    {
        private const double DevFee = 2.0;
        private HttpClient _httpClient;
        private int _apiPort;

        // GMiner can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;
        
        // command line parts
        private string _devices;

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.ZHash:
                    return "144_5";
                case AlgorithmType.Beam:
                    return "150_5";
                case AlgorithmType.GrinCuckaroo29:
                    return "grin29";
                default:
                    return "";
            }
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange(); // use the default range

            var algo = AlgorithmName(_algorithmType);

            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var split = urlWithPort.Split(':');
            var url = split[0];
            var port = split[1];

            var cmd = $"-a {algo} -s {url} -n {port} -u {username} -d {_devices} -w 0 --api {_apiPort}";

            if (_algorithmType == AlgorithmType.ZHash)
            {
                cmd += " --pers auto";
            }

            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            // lazy init
            if (_httpClient == null) _httpClient = new HttpClient();
            var ad = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/stat");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);                

                var gpus = _miningPairs.Select(pair => pair.device);
                var perDeviceSpeedInfo = new List<(string uuid, IReadOnlyList<(AlgorithmType, double)>)>();
                var perDevicePowerInfo = new List<(string, int)>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;
                foreach (var gpu in gpus)
                {
                    var currentDevStats = summary.devices.Where(devStats => devStats.gpu_id == gpu.ID).FirstOrDefault();
                    if (currentDevStats == null) continue;
                    totalSpeed += currentDevStats.speed;
                    perDeviceSpeedInfo.Add((gpu.UUID, new List<(AlgorithmType, double)>() { (_algorithmType, currentDevStats.speed) }));
                    totalPowerUsage += currentDevStats.power_usage;
                    perDevicePowerInfo.Add((gpu.UUID, currentDevStats.power_usage));
                }
                ad.AlgorithmSpeedsTotal = new List<(AlgorithmType, double)> { (_algorithmType, totalSpeed) };
                ad.PowerUsageTotal = totalPowerUsage;
            }
            catch (Exception e)
            {
                //CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
                //Helpers.ConsolePrint(MinerTag(), e.Message);
            }

            return ad;
        }

        public async override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {// determine benchmark time 
            // settup times
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

            // use demo user and disable the watchdog
            var commandLine = CreateCommandLine(MinerToolkit.DemoUser);
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            double benchHashesSum = 0;
            double benchHashResult = 0;
            int benchIters = 0;
            int targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 30d));
            // TODO implement fallback average, final benchmark 
            bp.CheckData = (string data) => {
                var (hashrate, found) = MinerToolkit.TryGetHashrateAfter(data, "Total Speed:");
                if (!found) return (benchHashResult, false);

                // sum and return
                benchHashesSum += hashrate;
                benchIters++;

                benchHashResult = (benchHashesSum / benchIters) * (1 - DevFee * 0.01);

                var isFinished = benchIters >= targetBenchIters;
                return (benchHashResult, isFinished);
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override (string, string) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), Shared.UUID);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "miner.exe");
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
            _devices = string.Join(" ", orderedMiningPairs.Select(p => p.device.ID));
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }
    }
}
