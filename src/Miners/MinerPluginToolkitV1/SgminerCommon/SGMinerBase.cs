using MinerPlugin;
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
using MinerPluginToolkitV1.Configs;

namespace MinerPluginToolkitV1.SgminerCommon
{
    public class SGMinerBase : MinerBase
    {
        private int _apiPort;
        // command line parts
        private string _devicesOnPlatform;

        public SGMinerBase(string uuid) : base(uuid)
        {}

        // override for your sgminer case
        protected virtual string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    //case AlgorithmType.NeoScrypt:
                    //    return "neoscrypt";
                    //case AlgorithmType.Keccak:
                    //    return "keccak";
                    case AlgorithmType.DaggerHashimoto:
                        return "ethash";
                    case AlgorithmType.X16R:
                        return "x16r";
                    default:
                        return "";
                }
            }
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var apiDevsResult = await SgminerAPIHelpers.GetApiDevsRootAsync(_apiPort, _logGroup);
            var devs = _miningPairs.Select(pair => pair.Device);
            return SgminerAPIHelpers.ParseApiDataFromApiDevsRoot(apiDevsResult, _algorithmType, devs, _logGroup);
        }

        protected override void Init()
        {
            // check platform id
            var openClAmdPlatformResult = MinerToolkit.GetOpenCLPlatformID(_miningPairs);
            var openClAmdPlatformNum = openClAmdPlatformResult.Item1;
            bool openClAmdPlatformNumUnique = openClAmdPlatformResult.Item2;
            if (!openClAmdPlatformNumUnique)
            {
                Logger.Error(_logGroup, "Initialization of miner failed. Multiple OpenCLPlatform IDs found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }
            // all good continue on

            // Order pairs and parse ELP
            var miningPairsList = _miningPairs.ToList();
            var deviceIds = miningPairsList.Select(pair => pair.Device.ID);
            _devicesOnPlatform = $"--gpu-platform {openClAmdPlatformNum} -d {string.Join(",", deviceIds)}";


            // if no MinerOptionsPackage fallback to defaults
            if (MinerOptionsPackage == null)
            {
                var ignoreDefaults = SgminerOptionsPackage.DefaultMinerOptionsPackage.IgnoreDefaultValueOptions;
                var generalParams = ExtraLaunchParametersParser.Parse(miningPairsList, SgminerOptionsPackage.DefaultMinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var temperatureParams = ExtraLaunchParametersParser.Parse(miningPairsList, SgminerOptionsPackage.DefaultMinerOptionsPackage.TemperatureOptions, ignoreDefaults);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // TODO dagger takes a long time
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // AVEMORE needs time to build kernels for each platform and this takes quite a while
            // TODO avemore takes REALLY LONG TIME TO BUILD KERNELS!!!! ADD kernel build checks
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 60, 90, 180 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            // use demo user and disable colorts so we can read from stdout
            var stopAt = DateTime.Now.AddSeconds(benchmarkTime).ToString("HH:mm");
            var commandLine = $"--sched-stop {stopAt} -T " + CreateCommandLine(MinerToolkit.DemoUserBTC);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var device = _miningPairs.Select(kvp => kvp.Device).FirstOrDefault();
            string currentGPU = $"GPU{device.ID}";
            const string hashrateAfter = "(avg):";

            bp.CheckData = (string data) =>
            {
                var containsHashRate = data.Contains(currentGPU) && data.Contains(hashrateAfter);
                if (containsHashRate == false) return new BenchmarkResult{Success = false};
                
                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, hashrateAfter);
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair>{ new AlgorithmTypeSpeedPair(_algorithmType, hashrate)},
                    Success = found
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
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "sgminer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected string CreateCommandLine(string username)
        {
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"-k {AlgoName} -o {url} -u {username} -p x {_extraLaunchParameters} {_devicesOnPlatform}";
            return cmd;
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            return CreateCommandLine(_username) + $" --api-listen --api-port={_apiPort}";
        }
    }
}
