using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NanoMiner
{
    public class NanoMiner : MinerBase
    {

        private readonly HttpClient _http = new HttpClient();
        private string _extraLaunchParameters = "";

        private int _apiPort;

        private AlgorithmType _algorithmType;

        private string _devices;

        public NanoMiner(string uuid) : base(uuid)
        {}

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.GrinCuckaroo29:
                    return "Cuckaroo29";
                case AlgorithmType.CryptoNightR:
                    return "CryptoNightR";
                default:
                    return "";
            }
        }

        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.CryptoNightR:
                        return 1.0;
                    case AlgorithmType.GrinCuckaroo29:
                    default: return 2.0;
                }
            }
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "nanominer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");

            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            _devices = string.Join(",", orderedMiningPairs.Select(p => p.Device.ID));
            //TODO this must be implemented
            if (MinerOptionsPackage != null)
            {
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/stats");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);
                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;
           
                foreach (var apiDeviceData in summary.Devices)
                {
                    foreach (var kvp in apiDeviceData)
                    {
                        var devId = int.Parse(kvp.Key.Remove(0, 3), System.Globalization.NumberStyles.HexNumber); //remove GPU from GPU XX string to get ID
                        var gpu = gpus.Where(dev => dev.ID == devId).FirstOrDefault();
                        var devData = kvp.Value;
                        var currentPower = Convert.ToInt32(devData.Power);
                        totalPowerUsage += currentPower;
                        perDevicePowerInfo.Add(gpu.UUID, currentPower);
                    }
                }

                foreach (var apiAlgoData in summary.Algorithms)
                {
                    foreach (var kvp in apiAlgoData)
                    {
                        var algo = kvp.Key;
                        var algoData = kvp.Value;
                        foreach (var data in algoData)
                        {
                            if (data.Key.Contains("GPU"))
                            {
                                var devId = int.Parse(data.Key.Remove(0, 3), System.Globalization.NumberStyles.HexNumber); //remove GPU from GPU XX string to get ID
                                var gpu = gpus.Where(dev => dev.ID == devId).FirstOrDefault();
                                var speed = data.Value.ToString();
                                perDeviceSpeedInfo.Add(gpu.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, JsonApiHelpers.HashrateFromApiData(speed) * (1 - DevFee * 0.01)) });
                            }
                            else if (data.Key == "Total")
                            {
                                var speed = data.Value.ToString();
                                totalSpeed = JsonApiHelpers.HashrateFromApiData(speed);
                            }
                        }
                    }
                }

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.PowerUsageTotal = totalPowerUsage;
            }
            catch (Exception e)
            {
                if (e.Message != "An item with the same key has already been added.")
                {
                    Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                }
            }

            return api;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            int benchmarkTime;
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 20;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    break;
                default:
                    benchmarkTime = 60;
                    break;
            }

            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC, _devices);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, "Total speed:");
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;

                if (found)
                {
                    benchHashes += hashrate;
                    benchIters++;

                    benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);
                }

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = benchIters >= targetBenchIters
                };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username, _devices);
        }

        private string CreateCommandLine(string username, string deviceId)
        {
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange();

            var algo = AlgorithmName(_algorithmType);

            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var paths = GetBinAndCwdPaths();

            var configString = "";
            if (_extraLaunchParameters != "")
            {
                var arrayOfELP = _extraLaunchParameters.Split(' ');
                foreach (var elp in arrayOfELP)
                {
                    configString += $"{elp}\r\n";
                }
            }

            configString += $"webPort={_apiPort}\r\nwatchdog=false\n\r\n\r[{algo}]\r\nwallet={username}\r\ndevices={_devices}\r\npool1={url}";
            File.WriteAllText(Path.Combine(paths.Item2,$"config_nh_{deviceId}.ini"), configString);
            return $"config_nh_{deviceId}.ini";
        }
    }
}
