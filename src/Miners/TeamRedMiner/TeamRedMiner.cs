using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static NHM.Common.StratumServiceHelpers;
using System.IO;
using NHM.Common;
using MinerPluginToolkitV1.SgminerCommon;
using MinerPluginToolkitV1.Configs;

namespace TeamRedMiner
{
    public class TeamRedMiner : MinerBase
    {
        private int _apiPort;
        private string _extraLaunchParameters = "";

        // can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;

        // the order of intializing devices is the order how the API responds
        private Dictionary<int, string> _initOrderMirrorApiOrderUUIDs = new Dictionary<int, string>();
        private int _openClAmdPlatformNum;
        // command line parts
        private string _devices;


        public TeamRedMiner(string uuid) : base(uuid)
        {
        }

        private string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.CryptoNightR:
                        return "cnr";
                    case AlgorithmType.Lyra2REv3:
                        return "lyra2rev3";
                    case AlgorithmType.X16R:
                        return "x16r";
                    case AlgorithmType.Lyra2Z:
                        return "lyra2z";
                    case AlgorithmType.GrinCuckatoo31:
                        return "cuckatoo31_grin";
                    default:
                        return "";
                }
            }
        }

        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.CryptoNightR:
                    case AlgorithmType.X16R:
                    case AlgorithmType.Lyra2REv3:
                    case AlgorithmType.GrinCuckatoo31:
                        return 2.5;
                    default:
                        return 3.0; 
                }
            }
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"-a {AlgoName} -o {url} -u {username} --platform={_openClAmdPlatformNum} -d {_devices} --api_listen=127.0.0.1:{_apiPort} {_extraLaunchParameters}";
            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var apiDevsResult = await SgminerAPIHelpers.GetApiDevsRootAsync(_apiPort, _logGroup);
            var ad = new ApiData();
            if (apiDevsResult == null) return ad;

            try
            {
                var deviveStats = apiDevsResult.DEVS;
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                // the devices have ordered ids by -d parameter, so -d 4,2 => 4=0;2=1
                foreach (var kvp in _initOrderMirrorApiOrderUUIDs)
                {
                    var gpuID = kvp.Key;
                    var gpuUUID = kvp.Value;
                    
                    var deviceStats = deviveStats
                        .Where(devStat => gpuID == devStat.GPU)
                        .FirstOrDefault();
                    if (deviceStats == null) continue;

                    var speedHS = deviceStats.KHS_av * 1000;
                    totalSpeed += speedHS;
                    perDeviceSpeedInfo.Add(gpuUUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, speedHS * (1 - DevFee * 0.01)) });
                    // TODO check PowerUsage API
                }
                ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 60, 90, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            // use demo user and disable colorts so we can read from stdout
            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC) + " --disable_colors";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            double benchHashesSum = 0;
            double benchHashResult = 0;
            int benchIters = 0;
            int targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 30d));

            string afterAlgoSpeed = $"{AlgoName}:";
            
            bp.CheckData = (string data) =>
            {
                var containsHashRate = data.Contains(afterAlgoSpeed) && data.Contains("GPU");
                if (containsHashRate == false) return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) }, Success = false };
                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, afterAlgoSpeed);
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

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "teamredminer-v0.5.5-win");
            var binPath = Path.Combine(pluginRootBins, "teamredminer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Info(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }
            // all good continue on

            // Order pairs and parse ELP
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            _devices = string.Join(",", orderedMiningPairs.Select(p => p.Device.ID));

            var openClAmdPlatformResult = MinerToolkit.GetOpenCLPlatformID(_miningPairs);
            _openClAmdPlatformNum = openClAmdPlatformResult.Item1;
            bool openClAmdPlatformNumUnique = openClAmdPlatformResult.Item2;
            if (!openClAmdPlatformNumUnique)
            {
                Logger.Error(_logGroup, "Initialization of miner failed. Multiple OpenCLPlatform IDs found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }

            for (int i = 0; i < orderedMiningPairs.Count; i++)
            {
                _initOrderMirrorApiOrderUUIDs[i] = orderedMiningPairs[i].Device.UUID;
            }

            if (MinerOptionsPackage != null)
            {
                var ignoreDefaults = MinerOptionsPackage.IgnoreDefaultValueOptions;
                var generalParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var temperatureParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions, ignoreDefaults);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }
    }
}
