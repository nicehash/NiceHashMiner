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

namespace NBMiner
{
    public class NBMiner : MinerBase, IDisposable
    {

        protected AlgorithmType _algorithmSecondType = AlgorithmType.NONE;
        private int _apiPort;
        private string _devices = "";
        private readonly HttpClient _http = new HttpClient();
        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private string AlgoName
        {
            get
            {
                if (_algorithmSecondType != AlgorithmType.NONE) return "eaglesong_ethash";
                return PluginSupportedAlgorithms.AlgorithmName(_algorithmType);
            }
        }

        private double DevFee
        {
            get
            {
                if (_algorithmSecondType != AlgorithmType.NONE) return 3.0;
                return PluginSupportedAlgorithms.DevFee(_algorithmType);
            }
        }


        public NBMiner(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedIDs = mappedIDs;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 20, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            // use demo user and disable the watchdog
            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());
            // disable line readings and read speeds from API
            bp.CheckData = null;

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);

            double benchHashesSum = 0;
            double benchHashesSum2 = 0;
            int benchIters = 0;
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
                        if (_algorithmSecondType == AlgorithmType.NONE)
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
                        else
                        {
                            var gpuSpeed = ad.AlgorithmSpeedsPerDevice.Values.FirstOrDefault().FirstOrDefault().Speed;
                            var gpuSpeed2 = ad.AlgorithmSpeedsPerDevice.Values.FirstOrDefault().LastOrDefault().Speed;
                            benchHashesSum += gpuSpeed;
                            benchHashesSum2 += gpuSpeed2;
                            benchIters++;
                            double benchHashResult = (benchHashesSum / benchIters); // fee is subtracted from API readings
                                                                                    // save each result step
                            double benchHashResult2 = (benchHashesSum2 / benchIters); // fee is subtracted from API readings
                                                                                    // save each result step
                            result = new BenchmarkResult
                            {
                                AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult), new AlgorithmTypeSpeedPair(_algorithmSecondType, benchHashResult2) },
                                Success = benchIters >= (ticks - 1) // allow 1 tick to fail and still consider this benchmark as success
                            };
                        }
                        
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

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            // NVIDIA only platform
            if (_algorithmSecondType == AlgorithmType.NONE)
            {
                return $"-a {AlgoName} -o {url} -u {username} --api 127.0.0.1:{_apiPort} {_devices} --no-watchdog --platform 1 {_extraLaunchParameters}";
            }
            var url2 = StratumServiceHelpers.GetLocationUrl(_algorithmSecondType, _miningLocation, NhmConectionType.NONE);
            return $"-a {AlgoName} -o {url} -u {username} -do nicehash+tcp://{url2} -du {username} --api 127.0.0.1:{_apiPort} {_devices} --no-watchdog --platform 1 {_extraLaunchParameters}";
        }
        
        public override async Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/api/v1/status");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalSpeed2 = 0d;
                var totalPowerUsage = 0;

                var apiDevices = summary.miner.devices;

                foreach (var miningPair in _miningPairs)
                {
                    var deviceUUID = miningPair.Device.UUID;
                    var minerID = _mappedIDs[deviceUUID];
                    var apiDevice = apiDevices.Find(apiDev => apiDev.id == minerID);
                    if (apiDevice == null) continue;

                    totalSpeed += apiDevice.hashrate_raw;
                    totalSpeed2 += apiDevice.hashrate2_raw;
                    var kPower = (int)apiDevice.power * 1000;
                    totalPowerUsage += kPower;
                    if (_algorithmSecondType == AlgorithmType.NONE)
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, apiDevice.hashrate_raw * (1 - DevFee * 0.01)) });
                    }
                    else
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<AlgorithmTypeSpeedPair> {
                            new AlgorithmTypeSpeedPair(_algorithmType, apiDevice.hashrate_raw * (1 - DevFee * 0.01)),
                            new AlgorithmTypeSpeedPair(_algorithmSecondType, apiDevice.hashrate2_raw * (1 - DevFee * 0.01)) });
                    }
                    perDevicePowerInfo.Add(deviceUUID, kPower);
                }
                if (_algorithmSecondType == AlgorithmType.NONE)
                {
                    api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                }
                else
                {
                    api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> {
                            new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)),
                            new AlgorithmTypeSpeedPair(_algorithmSecondType, totalSpeed2 * (1 - DevFee * 0.01)) };
                }
                api.PowerUsageTotal = totalPowerUsage;
                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by mapped ids
            pairsList.Sort((a, b) => _mappedIDs[a.Device.UUID].CompareTo(_mappedIDs[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            var devs = string.Join(",", _miningPairs.Select(p => _mappedIDs[p.Device.UUID]));
            _devices = $"-d {devs}";

            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmSecondType = dualType.Item1;
            var ok = dualType.Item2;
            if (!ok) _algorithmSecondType = AlgorithmType.NONE;
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
