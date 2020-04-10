using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SRBMiner
{
    public class SRBMiner : MinerBase
    {
        private int _apiPort;
        private double DevFee = 0.85;
        private string _devices;
        private HttpClient _httpClient;
        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public SRBMiner(string uuid, Dictionary<string, int> mappedDeviceIds) : base(uuid)
        {
            _mappedDeviceIds = mappedDeviceIds;
        }

        private string AlgoName => PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            // lazy init
            if (_httpClient == null) _httpClient = new HttpClient();
            var ad = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}");
                var summary = JsonConvert.DeserializeObject<ApiJsonResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                var amdDevices = gpus.Cast<AMDDevice>();
                foreach (var gpu in amdDevices)
                {
                    var currentDevStats = summary.gpu_devices.Where(dev => dev.bus_id == gpu.PCIeBusID).FirstOrDefault();
                    if (currentDevStats == null) continue;
                    var device = currentDevStats.device;
                    var data = summary.gpu_hashrate[0].TryGetValue(device, out var currentSpeed);
                    totalSpeed += currentSpeed;
                    perDeviceSpeedInfo.Add(gpu.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentSpeed * (1 - DevFee * 0.01)) });
                }
                ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                //CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
            }

            return ad;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 20, 40, 60 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());
            // disable line readings and read speeds from API
            bp.CheckData = null;

            var timeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, timeout, benchWait, stop);

            double benchHashesSum = 0;
            var benchIters = 0;
            var ticks = benchmarkTime / 10; // on each 10 seconds tick
            var result = new BenchmarkResult();
            for (var tick = 0; tick < ticks; tick++)
            {
                if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;
                await Task.Delay(10 * 1000, stop); // 10 seconds delay
                if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;

                var ad = await GetMinerStatsDataAsync();
                if (ad.AlgorithmSpeedsPerDevice.Count == 1)
                {
                    // all single GPUs and single speeds
                    try
                    {
                        var gpuSpeed = ad.AlgorithmSpeedsPerDevice.Values.FirstOrDefault().FirstOrDefault().Speed;
                        benchHashesSum += gpuSpeed;
                        benchIters++;
                        double benchHashResult = (benchHashesSum / benchIters); // fee is subtracted from API readings
                                                                                // save each result step
                        result = new BenchmarkResult
                        {
                            AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                            Success = benchIters >= (ticks - 1) // allow 1 tick to fail and still consider this benchmark as success
                        };
                    }
                    catch (Exception e)
                    {
                        if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;
                        Logger.Error(_logGroup, $"benchmarking error: {e.Message}");
                    }
                }
            }
            // await benchmark task
            await t;
            if (stop.IsCancellationRequested)
            {
                return t.Result;
            }

            // return API result
            return result;
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
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"--algorithm {AlgoName} --wallet {_username} --gpu-id {_devices} --pool {urlWithPort} --disable-cpu --disable-gpu-watchdog --api-enable --api-port {_apiPort} {_extraLaunchParameters}";
            return cmd;
        }
    }
}
