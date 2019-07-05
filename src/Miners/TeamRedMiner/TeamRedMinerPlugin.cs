using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPluginToolkitV1.Interfaces;
using System.IO;
using NiceHashMinerLegacy.Common;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1;
using System.Threading.Tasks;

namespace TeamRedMiner
{
    public class TeamRedMinerPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2
    {
        public TeamRedMinerPlugin()
        {
            _pluginUUID = "abc3e2a0-7237-11e9-b20c-f9f12eb6d835";
        }
        public TeamRedMinerPlugin(string pluginUUID = "abc3e2a0-7237-11e9-b20c-f9f12eb6d835")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 3);

        public string Name => "TeamRedMiner";

        public string Author => "stanko@nicehash.com";

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            if (a.Device is AMDDevice aDev && b.Device is AMDDevice bDev && aDev.OpenCLPlatformID != bDev.OpenCLPlatformID)
            {
                // OpenCLPlatorm IDs must match
                return false;
            }
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new TeamRedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
            };
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // Get AMD GCN4+
            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm> {
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightR),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2Z),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
            };
            return algorithms;
        }

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerReservedPorts = InternalConfigs.InitMinerReservedPorts(pluginRoot, _minerReservedApiPorts);
            if (fileMinerReservedPorts != null) _minerReservedApiPorts = fileMinerReservedPorts;

            var fileMinerApiMaxTimeoutSetting = InternalConfigs.InitMinerApiMaxTimeoutSetting(pluginRoot, _getApiMaxTimeoutConfig);
            if (fileMinerApiMaxTimeoutSetting != null) _getApiMaxTimeoutConfig = fileMinerApiMaxTimeoutSetting;
        }

        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_USE_SYNC_OBJECTS", "1"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
            },
        };

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage{
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Specified the init style (1 is default):
                /// 1: One gpu at the time, complete all before mining.
                /// 2: Three gpus at the time, complete all before mining.
                /// 3: All gpus in parallel, start mining immediately.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_initStyle",
                    ShortName = "--init_style=",
                    DefaultValue = "1"
                },
                /// <summary>
                /// Enables debug log output.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_debug",
                    ShortName = "--debug",
                },
                /// <summary>
                /// Set the time interval in seconds for averaging and printing GPU hashrates.
                /// SEC sets the interval in seconds, and must be > 0.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_logInterval",
                    ShortName = "-l",
                    LongName = "--log_interval="
                },
                /// <summary>
                /// Manual cryptonight configuration for the miner.  CONFIG must be in the form
                /// [P][I0][M][I1][:xyz], where [P] is an optional prefix and [:xyz] is an
                /// optional suffix.  For [P], only the value of 'L' is supported for low-end
                /// GPUs like Lexa/Baffin.  [I0] and [I1] are the thread intensity values normally
                /// ranging from 1 to 16, but larger values are possible for 16GB gpus.  [M] is the
                /// mode which can be either '.', -', '+' or '*'.  Mode '.' means that the miner
                /// should choose or scan for the best mode.  Mode '*' both a good default more and
                /// _should_ be used if you mine on a Vega 56/64 with modded mem timings.  The
                /// exceptions to this rule are small pad variants (cnv8_trtl and cnv8_upx2), they
                /// should still use '+'. For Polaris gpus, only the '-' and '+' modes are available.
                /// NOTE: in TRM 0.5.0 auto-tuning functionality was added, making manual configuration
                /// of the CN config modes unnecessary except for rare corner cases.  For more info,
                /// see the tuning docs and how-to documents bundled with the release.
                /// Example configs: --cn_config=15*15:AAA
                /// --cn_config=L4+3
                /// CONFIG can also be a comma seperated list of config values where each is
                /// applied to each GPU. For example: --cn_config=8-8,16+14:CBB,15*15,14-14
                /// Any gpu that does not have a specific config in the list will use the first
                /// config in the list.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_cnConfig",
                    ShortName = "--cn_config=",
                },
                /// <summary>
                /// Disables cpu verification of found shares before they are submitted to the pool.
                /// Note: only CN algos currently supports cpu verification.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_noCpuCheck",
                    ShortName = "--no_cpu_check",
                },
                /// <summary>
                /// Disables the CN lean mode where ramp up threads slowly on start or restart after
                /// network issues or gpu temp throttling.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_noLean",
                    ShortName = "--no_lean",
                },
                /// <summary>
                /// Lists gpu devices where CN thread interleave logic should be not be used.
                /// The argument is a comma-separated list of devices like for the -d option.
                /// Use this argument if some device(s) get a worse hashrate together with a lot
                /// of interleave adjust log messages.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_noInterleave",
                    ShortName = "--no_interleave=",
                },
                /// <summary>
                /// Enable the auto-tune mode upon startup. Only available for CN variants. MODE must
                /// be either NONE, QUICK or SCAN. The QUICK mode checks a few known good configurations
                /// and completes within 1 min. The SCAN mode will check all possible combos and will
                /// run for 20-30 mins. Setting MODE to NONE disable the auto-tune feature. The default
                /// mode is QUICK.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_autoTuneMode",
                    ShortName = "--auto_tune=",
                    DefaultValue = "QUICK"
                },
                /// <summary>
                /// Executes multiple runs for the auto tune, each time decreasing the unit of pads used -1
                /// in one of the threads (15+15 -> 15+14 -> 14+14 -> 14+13 -> ...). You can specify the
                /// explicit nr of runs or let the miner choose a default value per gpu type (typically 3-4).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_autoTuneRuns",
                    ShortName = "--auto_tune_runs=",
                },
                /// <summary>
                /// If present, and when the driver indicates there is enough GPU vram available, the miner
                /// will be more aggressive with the initial memory allocation. In practice, this option
                /// means that Vega GPUs under Linux will start the auto-tuning process at 16*15 rather
                /// than 16*14 or 15*15.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_largeAlloc",
                    ShortName = "--allow_large_alloc",
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                ///  Sets the temperature at which the miner will stop GPUs that are too hot.
                ///  Default is 85C.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_tempLimit",
                    ShortName = "--temp_limit=",
                    DefaultValue = "85"
                },
                /// <summary>
                ///  Sets the temperature below which the miner will resume GPUs that were previously stopped due to temperature exceeding limit.
                ///  Default is 60C.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_tempResume",
                    ShortName = "--temp_resume=",
                    DefaultValue = "60"
                }
            }
        };

        protected static MinerApiMaxTimeoutSetting _getApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout =  _defaultTimeout,
        };
        #endregion Internal Settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "teamredminer.exe" });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no improvements for algorithm speeds in the new version - just stability improvements
            return false;
        }

        #region IGetApiMaxTimeoutV2
        public bool IsGetApiMaxTimeoutEnabled => MinerApiMaxTimeoutSetting.ParseIsEnabled(true, _getApiMaxTimeoutConfig);

        protected static TimeSpan _defaultTimeout = new TimeSpan(0, 5, 0);
        public TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return MinerApiMaxTimeoutSetting.ParseMaxTimeout(_defaultTimeout, _getApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2
    }
}
