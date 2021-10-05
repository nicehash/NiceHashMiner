using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NHM.MinerPluginToolkitV1.Configs;

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

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);

            if (_algorithmSecondType == AlgorithmType.NONE && _algorithmType != AlgorithmType.DaggerHashimoto)
            {
                return $"-a {AlgoName} -o {url} -u {username} --api 127.0.0.1:{_apiPort} {_devices} --no-watchdog {_extraLaunchParameters}";
            }
            if (_algorithmSecondType == AlgorithmType.NONE && _algorithmType == AlgorithmType.DaggerHashimoto)
            {
                var url_dagger = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
                return $"-a {AlgoName} -o nicehash+tcp://{url_dagger} -u {username} --api 127.0.0.1:{_apiPort} {_devices} --no-watchdog {_extraLaunchParameters}";
            }
            var url2 = StratumServiceHelpers.GetLocationUrl(_algorithmSecondType, _miningLocation, NhmConectionType.NONE);
            var cmd = $"-a {AlgoName} -o {url} -u {username} -do nicehash+tcp://{url2} -du {username} --api 127.0.0.1:{_apiPort} {_devices} --no-watchdog {_extraLaunchParameters}";
            if (!_extraLaunchParameters.Contains("--secondary-intensity"))
            {
                cmd += " --secondary-intensity 100";
            }
            return cmd;
        }

        public override async Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/api/v1/status");
                api.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
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
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)> { (_algorithmType, apiDevice.hashrate_raw * (1 - DevFee * 0.01)) });
                    }
                    else
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)> {
                            (_algorithmType, apiDevice.hashrate_raw * (1 - DevFee * 0.01)),
                            (_algorithmSecondType, apiDevice.hashrate2_raw * (1 - DevFee * 0.01)) });
                    }
                    perDevicePowerInfo.Add(deviceUUID, kPower);
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

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            using (var tickCancelSource = new CancellationTokenSource())
            {
                // determine benchmark time 
                // settup times
                int benchmarkTime;
                if(_miningPairs.Any(dev => dev.Algorithm.AlgorithmName == "DaggerHashimoto" && dev.Algorithm.ExtraLaunchParameters.Contains("-LHR")))
                {
                    benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 60, 120, 240 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType);
                }
                else
                {
                    benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 40, 60, 140 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType);
                }
                var maxTicks = MinerBenchmarkTimeSettings.ParseBenchmarkTicks(new List<int> { 1, 3, 9 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType);
                var maxTicksEnabled = MinerBenchmarkTimeSettings.MaxTicksEnabled;

                //// use demo user and disable the watchdog
                var commandLine = MiningCreateCommandLine();
                var (binPath, binCwd) = GetBinAndCwdPaths();
                Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
                Logger.Info(_logGroup, $"Benchmarking settings: time={benchmarkTime} ticks={maxTicks} ticksEnabled={maxTicksEnabled}");
                var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());
                // disable line readings and read speeds from API
                bp.CheckData = null;

                var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
                var benchmarkWait = TimeSpan.FromMilliseconds(500);
                var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop, tickCancelSource.Token);

                var stoppedAfterTicks = false;
                var validTicks = 0;
                var ticks = benchmarkTime / 10; // on each 10 seconds tick
                var result = new BenchmarkResult();
                var benchmarkApiData = new List<ApiData>();
                for (var tick = 0; tick < ticks; tick++)
                {
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;
                    await Task.Delay(10 * 1000, stop); // 10 seconds delay
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;

                    var ad = await GetMinerStatsDataAsync();
                    var adTotal = ad.AlgorithmSpeedsTotal();
                    var isTickValid = adTotal.Count > 0 && adTotal.All(pair => pair.speed > 0);
                    benchmarkApiData.Add(ad);
                    if (isTickValid) ++validTicks;
                    if (maxTicksEnabled && validTicks >= maxTicks)
                    {
                        stoppedAfterTicks = true;
                        break;
                    }
                }
                // await benchmark task
                if (stoppedAfterTicks)
                {
                    try
                    {
                        tickCancelSource.Cancel();
                    }
                    catch
                    { }
                }
                await t;
                if (stop.IsCancellationRequested)
                {
                    return t.Result;
                }

                // calc speeds
                // TODO calc std deviaton to reduce invalid benches
                try
                {
                    var nonZeroSpeeds = benchmarkApiData.Where(ad => ad.AlgorithmSpeedsTotal().Count > 0 && ad.AlgorithmSpeedsTotal().All(pair => pair.speed > 0))
                                                        .Select(ad => (ad, ad.AlgorithmSpeedsTotal().Count)).ToList();
                    var speedsFromTotals = new List<(AlgorithmType type, double speed)>();
                    if (nonZeroSpeeds.Count > 0)
                    {
                        var maxAlgoPiarsCount = nonZeroSpeeds.Select(adCount => adCount.Count).Max();
                        var sameCountApiDatas = nonZeroSpeeds.Where(adCount => adCount.Count == maxAlgoPiarsCount).Select(adCount => adCount.ad).ToList();
                        var firstPair = sameCountApiDatas.FirstOrDefault();
                        var speedSums = firstPair.AlgorithmSpeedsTotal().Select(pair => new KeyValuePair<AlgorithmType, double>(pair.type, 0.0)).ToDictionary(x => x.Key, x => x.Value);
                        // sum 
                        foreach (var ad in sameCountApiDatas)
                        {
                            foreach (var pair in ad.AlgorithmSpeedsTotal())
                            {
                                speedSums[pair.type] += pair.speed;
                            }
                        }
                        // average
                        foreach (var algoId in speedSums.Keys.ToArray())
                        {
                            speedSums[algoId] /= sameCountApiDatas.Count;
                        }
                        result = new BenchmarkResult
                        {
                            AlgorithmTypeSpeeds = firstPair.AlgorithmSpeedsTotal().Select(pair => (pair.type, speedSums[pair.type])).ToList(),
                            Success = true
                        };
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(_logGroup, $"benchmarking AlgorithmSpeedsTotal error {e.Message}");
                }

                // return API result
                return result;
            }

        }
    }
}
