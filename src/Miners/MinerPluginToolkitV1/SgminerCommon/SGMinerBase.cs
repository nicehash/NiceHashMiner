using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
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
        
        // can mine only one algorithm at a given time
        protected AlgorithmType _algorithmType;

        // command line parts
        private string _devicesOnPlatform;
        private string _extraLaunchParameters;

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
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Error(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }
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
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            var deviceIds = orderedMiningPairs.Select(pair => pair.Device.ID);
            _devicesOnPlatform = $"--gpu-platform {openClAmdPlatformNum} -d {string.Join(",", deviceIds)}";


            if (MinerOptionsPackage != null)
            {
                var ignoreDefaults = MinerOptionsPackage.IgnoreDefaultValueOptions;
                var generalParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var temperatureParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions, ignoreDefaults);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
            else // TODO this one is temp???
            {
                var ignoreDefaults = DefaultMinerOptionsPackage.IgnoreDefaultValueOptions;
                var generalParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, DefaultMinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var temperatureParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, DefaultMinerOptionsPackage.TemperatureOptions, ignoreDefaults);
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


        // TODO remove redundant/duplicated long/short names after ELP parser is fixed
        public static readonly MinerOptionsPackage DefaultMinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                // Single Param
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "KeccakUnroll",
                    ShortName = "--keccak-unroll",
                    LongName = "--keccak-unroll",
                    DefaultValue = "0"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "HamsiExpandBig",
                    ShortName = "--hamsi-expand-big",
                    LongName = "--hamsi-expand-big",
                    DefaultValue = "4"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "Nfactor",
                    ShortName = "--nfactor",
                    LongName = "--nfactor",
                    DefaultValue = "10"
                },
                // Multi Params
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Intensity",
                    ShortName = "-I",
                    LongName = "--intensity",
                    DefaultValue = "d",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Xintensity",
                    ShortName = "-X",
                    LongName = "--xintensity",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Rawintensity",
                    ShortName = "--rawintensity",
                    LongName = "--rawintensity",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ThreadConcurrency",
                    ShortName = "--thread-concurrency",
                    LongName = "--thread-concurrency",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Worksize",
                    ShortName = "-w",
                    LongName = "--worksize",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "GpuThreads",
                    ShortName = "-g",
                    LongName = "--gpu-threads",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "LookupGap",
                    ShortName = "--lookup-gap",
                    LongName= "--lookup-gap",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                // Only parameter
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "RemoveDisabled",
                    ShortName = "--remove-disabled",
                    DefaultValue = "--remove-disabled",
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "GpuFan",
                    ShortName = "--gpu-fan",
                    LongName = "--gpu-fan",
                    DefaultValue = "30-60",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "TempCutoff",
                    ShortName = "--temp-cutoff",
                    LongName = "--temp-cutoff",
                    DefaultValue = "95",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "TempOverheat",
                    ShortName = "--temp-overheat",
                    LongName = "--temp-overheat",
                    DefaultValue = "85",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "TempTarget",
                    ShortName = "--temp-target",
                    LongName = "--temp-target",
                    DefaultValue = "75",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "AutoFan",
                    ShortName = "--auto-fan",
                    LongName = "--auto-fan",
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "AutoGpu",
                    ShortName = "--auto-gpu",
                    LongName = "--auto-gpu",
                },
            }
        };
    }
}
