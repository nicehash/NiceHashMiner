using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace XMRig
{
    public class XMRig : MinerBase, IBeforeStartMining
    {
        // TODO DevFee 
        private double DevFee = 1.0;
        private int _apiPort;
        protected readonly HttpClient _httpClient = new HttpClient();

        private string AlgoName
        {
            get
            {
                return PluginSupportedAlgorithms.AlgorithmName(_algorithmType);
            }
        }

        public XMRig(string uuid) : base(uuid) { }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/1/summary");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var totalSpeed = 0d;
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                // init per device sums
                foreach (var pair in _miningPairs)
                {
                    var uuid = pair.Device.UUID;
                    var currentSpeed = summary.hashrate.total.FirstOrDefault() ?? 0d;
                    totalSpeed += currentSpeed;
                    perDeviceSpeedInfo.Add(uuid, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentSpeed * (1 - DevFee * 0.01)) });
                    // no power usage info
                    perDevicePowerInfo.Add(uuid, -1);
                }

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                api.PowerUsageTotal = -1;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerPluginToolkitV1.Configs.MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 30, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            // use demo user and disable the watchdog
            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC) + "--print-time=10";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine} --print-time=10");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            double benchHashesSum = 0;
            double benchHashResult = 0;
            int benchIters = 0;
            int targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 10d));
            // TODO implement fallback average, final benchmark 
            bp.CheckData = (string data) => {
                var hashrateFoundPair = BenchmarkHelpers.TryGetHashrateAfter(data, "speed");
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;
                if (!found) return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) }, Success = false };

                // sum and return
                benchHashesSum += hashrate;
                benchIters++;

                benchHashResult = (benchHashesSum / benchIters) * (1 - DevFee * 0.01);

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = benchIters >= targetBenchIters
                };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override void Init()
        {
            if (_extraLaunchParameters.Contains("--donate-level="))
            {
                var splittedELP = _extraLaunchParameters.Split(' ');
                try
                {
                    foreach (var elp in splittedELP)
                    {
                        if (elp.Contains("--donate-level="))
                        {
                            var parsedDevFee = elp.Split('=')[1];
                            double.TryParse(parsedDevFee, out var devFee);
                            DevFee = devFee;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(_logGroup, $"Init failed: {e.Message}");
                }
            }
            else
            {
                _extraLaunchParameters += " --donate-level=1";
            }
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var cmd = "";
            //if user wants to manually tweek with config file we let him do that - WARNING some functionalities might not work (benchmarking, api data)
            if (_extraLaunchParameters.Contains("--config="))
            {
                cmd = _extraLaunchParameters;
            }
            else
            {
                cmd = $"-a {AlgoName} -o {urlWithPort} -u {username} --http-enabled --http-port={_apiPort} --nicehash {_extraLaunchParameters}";
            }
            Logger.Info("STARTED", $"command: {cmd}");
            return cmd;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }


        private static HashSet<string> _deleteConfigs = new HashSet<string> { "config.json" };
        private static bool IsDeleteConfigFile(string file)
        {
            foreach (var conf in _deleteConfigs)
            {
                if (file.Contains(conf)) return true;
            }
            return false;
        }
        void IBeforeStartMining.BeforeStartMining()
        {
            //if user wants to manually tweek with config file we let him do that - WARNING some functionalities might not work (benchmarking, api data)
            if (_extraLaunchParameters.Contains("--config="))
            {
                return;
            }
            var binCwd = GetBinAndCwdPaths().Item2;
            var txtFiles = Directory.GetFiles(binCwd, "*.json", SearchOption.AllDirectories)
                .Where(file => IsDeleteConfigFile(file))
                .ToArray();
            foreach (var deleteFile in txtFiles)
            {
                try
                {
                    File.Delete(deleteFile);
                }
                catch (Exception e)
                {
                    Logger.Error(_logGroup, $"BeforeStartMining error while deleting file '{deleteFile}': {e.Message}");
                }
            }
        }
    }
}
