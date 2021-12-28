﻿using Newtonsoft.Json;
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

namespace LolMiner
{
    public class LolMiner : MinerBase
    {
        private string _devices;
        private int _apiPort;
        private double DevFee = 1d;

        private string _disableWatchdogParam = "--disablewatchdog 1";

        // the order of intializing devices is the order how the API responds
        private Dictionary<int, string> _initOrderMirrorApiOrderUUIDs = new Dictionary<int, string>();
        protected Dictionary<string, int> _mappedIDs;

        private readonly HttpClient _http = new HttpClient();

        public LolMiner(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedIDs = mappedIDs;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                ad.ApiResponse = summaryApiResult;
                var summary = JsonConvert.DeserializeObject<ApiJsonResponse>(summaryApiResult);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var speedUnit = summary.Session.Performance_Unit;
                var multiplier = 1;
                switch (speedUnit)
                {
                    case "mh/s":
                        multiplier = 1000000; //1M
                        break;
                    case "kh/s":
                        multiplier = 1000; //1k
                        break;
                    default:
                        break;
                }
                var totalSpeed = summary.Session.Performance_Summary * multiplier;

                var totalPowerUsage = 0;
                var perDevicePowerInfo = new Dictionary<string, int>();

                var apiDevices = summary.GPUs;

                foreach (var pair in _miningPairs)
                {
                    var gpuUUID = pair.Device.UUID;
                    var gpuID = _mappedIDs[gpuUUID];
                    var currentStats = summary.GPUs.Where(devStats => devStats.Index == gpuID).FirstOrDefault();
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add(gpuUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentStats.Performance * multiplier * (1 - DevFee * 0.01)) });
                }

                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsageTotal = totalPowerUsage;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
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
            _devices = string.Join(",", _miningPairs.Select(p => _mappedIDs[p.Device.UUID]));

            // ???????? GetSortedMiningPairs is now sorted so this thing probably makes no sense anymore
            var miningPairs = _miningPairs.ToList();
            for (int i = 0; i < miningPairs.Count; i++)
            {
                _initOrderMirrorApiOrderUUIDs[i] = miningPairs[i].Device.UUID;
            }
            // if ELP contains watchdog remove default wd-param
            if (_extraLaunchParameters.Contains("--disablewatchdog"))
            {
                _disableWatchdogParam = "";
            }
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);

            var algo = AlgorithmName(_algorithmType);
            var commandLine = $"--pool {urlWithPort} --user {_username} --tls 0 --apiport {_apiPort} {_disableWatchdogParam} --devices {_devices} {_extraLaunchParameters}";

            if (_algorithmType == AlgorithmType.ZHash) commandLine += " --coin AUTO144_5";
            else commandLine += $" --algo {algo}";
            if (_algorithmType == AlgorithmType.DaggerHashimoto) commandLine += " --ethstratum ETHV1";
            //--disablewatchdog 1
            return commandLine;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var isDaggerNvidia = _miningPairs.Any(mp => mp.Algorithm.FirstAlgorithmType == AlgorithmType.DaggerHashimoto) && _miningPairs.Any(mp => mp.Device.DeviceType == DeviceType.NVIDIA);
            if (!isDaggerNvidia) return await base.StartBenchmark(stop, benchmarkType);

            using (var tickCancelSource = new CancellationTokenSource())
            {
                // determine benchmark time 
                // settup times
                
                int benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 180, 240, 300 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); ;
                var maxTicks = MinerBenchmarkTimeSettings.ParseBenchmarkTicks(new List<int> { 1, 3, 9 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType);

                //// use demo user and disable the watchdog
                var commandLine = MiningCreateCommandLine();
                var (binPath, binCwd) = GetBinAndCwdPaths();
                Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
                Logger.Info(_logGroup, $"Benchmarking settings: time={benchmarkTime} ticks={maxTicks}");
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
                var delay = (benchmarkTime / maxTicks) * 1000;

                for (var tick = 0; tick < ticks; tick++)
                {
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;
                    await Task.Delay(delay, stop); // 10 seconds delay
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;

                    var ad = await GetMinerStatsDataAsync();
                    var adTotal = ad.AlgorithmSpeedsTotal();
                    var isTickValid = adTotal.Count > 0 && adTotal.All(pair => pair.speed > 0);
                    benchmarkApiData.Add(ad);
                    if (isTickValid) ++validTicks;
                    if (validTicks >= maxTicks)
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
                        // sum 
                        var values = sameCountApiDatas.SelectMany(x => x.AlgorithmSpeedsTotal()).Select(pair => pair.speed).Reverse().Take(10).ToArray();
                        var value = values.Sum() / values.Length;
                        result = new BenchmarkResult
                        {
                            AlgorithmTypeSpeeds = firstPair.AlgorithmSpeedsTotal().Select(pair => (pair.type, value)).ToList(),
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
