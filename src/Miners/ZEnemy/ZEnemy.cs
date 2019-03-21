using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.Net.Http;
using System.IO;
using NiceHashMinerLegacy.Common;

namespace ZEnemy
{
    public class ZEnemy : MinerBase
    {
        private string _devices;

        private string _extraLaunchParameters = "";

        private int _apiPort;

        private readonly string _uuid;

        private AlgorithmType _algorithmType;

        private ApiDataHelper apiReader = new ApiDataHelper(); // consider replacing with HttpClient

        public ZEnemy(string uuid)
        {
            _uuid = uuid;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.Skunk: return "skunk";
            }
            return "";
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
            var api = new ApiData();
            try
            {
                var summaryApiResult = await apiReader.GetApiDataAsync(_apiPort, ApiDataHelper.GetHttpRequestNhmAgentStrin("summary"));
                double totalSpeed = 0;
                int totalPower = 0;
                if (!string.IsNullOrEmpty(summaryApiResult))
                {

                    var summaryOptvals = summaryApiResult.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var optvalPairs in summaryOptvals)
                    {
                        var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pair.Length != 2) continue;
                        if (pair[0] == "KHS")
                        {
                            totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                        }
                    }
                }

                var threadsApiResult = await apiReader.GetApiDataAsync(_apiPort, ApiDataHelper.GetHttpRequestNhmAgentStrin("threads"));
                var perDeviceSpeedInfo = new List<(string uuid, IReadOnlyList<(AlgorithmType, double)>)>();
                var perDevicePowerInfo = new List<(string, int)>();
                if (!string.IsNullOrEmpty(threadsApiResult))
                {

                    var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var gpu in gpus)
                    {
                        var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var gpuData = (id: -1, power: -1, speed: -1d);
                        foreach (var optvalPairs in gpuOptvalPairs)
                        {
                            var optval = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (optval.Length != 2) continue;
                            if (optval[0] == "GPU")
                            {
                                gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "POWER")
                            {
                                gpuData.power = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "KHS")
                            {
                                gpuData.speed = double.Parse(optval[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            }
                        }

                        var device = _miningPairs.Where(kvp => kvp.device.ID == gpuData.id).Select(kvp => kvp.device).FirstOrDefault();
                        if (device != null)
                        {
                            perDeviceSpeedInfo.Add((device.UUID, new List<(AlgorithmType, double)>() { (_algorithmType, gpuData.speed) }));
                            perDevicePowerInfo.Add((device.UUID, gpuData.power));
                            totalPower += gpuData.power;
                        }

                    }

                }
                var total = new List<(AlgorithmType, double)>();
                total.Add((_algorithmType, totalSpeed));
                api.AlgorithmSpeedsTotal = total;
                api.PowerUsageTotal = totalPower;
                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
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
            var benchmarkTime = 60;
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 40;
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

            var commandLine = $"--algo {algo} --url={url}:{port} --user {MinerToolkit.DemoUser} --devices {_devices} {_extraLaunchParameters}";
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            var after = $"GPU#"; //if multiple benchmark add gpu cuda id
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var hasHashRate = data.Contains(after) && data.Contains("-");

                if (!hasHashRate) return (benchHashResult, false);

                var (hashrate, found) = data.TryGetHashrateAfter("-");

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
            var binPath = Path.Combine(pluginRootBins, "z-enemy.exe");
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
            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url={url}:{port} --user {_username} --api-bind={_apiPort} --devices {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
