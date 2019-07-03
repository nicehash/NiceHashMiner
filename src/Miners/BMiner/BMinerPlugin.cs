using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BMiner
{
    public class BMinerPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2
    {
        // mandatory constructor
        public BMinerPlugin()
        {
            _pluginUUID = "e5fbd330-7235-11e9-b20c-f9f12eb6d835";
        }
        public BMinerPlugin(string pluginUUID = "e5fbd330-7235-11e9-b20c-f9f12eb6d835")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 4);
        public string Name => "BMiner";

        public string Author => "domen.kirnkrefl@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();
            foreach (var gpu in amdGpus)
            {
                var algorithms = GetAMDSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }
            // CUDA 9.2+ driver 397.44
            var mininumRequiredDriver = new Version(397, 44);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS >= mininumRequiredDriver)
            {
                var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();
                foreach (var gpu in cudaGpus)
                {
                    var algos = GetCUDASupportedAlgorithms(gpu).ToList();
                    if (algos.Count > 0) supported.Add(gpu, algos);
                }
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetCUDASupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.ZHash) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.Beam) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        private IEnumerable<Algorithm> GetAMDSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.Beam) {Enabled = false },
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public IMiner CreateMiner()
        {
            return new BMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        #region Internal settings
        public void InitInternals()
        {
            var pluginRoot = Paths.MinerPluginsPath(PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;

            var fileMinerReservedPorts = InternalConfigs.InitMinerReservedPorts(pluginRoot, _minerReservedApiPorts);
            if (fileMinerReservedPorts != null) _minerReservedApiPorts = fileMinerReservedPorts;

            var fileMinerApiMaxTimeoutSetting = InternalConfigs.InitMinerApiMaxTimeoutSetting(pluginRoot, _getApiMaxTimeoutConfig);
            if (fileMinerApiMaxTimeoutSetting != null) _getApiMaxTimeoutConfig = fileMinerApiMaxTimeoutSetting;
        }

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// The sub-solver for dual mining. Valid values are 0, 1, 2, 3. Default is -1, which is to tune automatically. (default -1)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_dual_subsolver",
                    ShortName = "-dual-subsolver",
                    DefaultValue = "-1"
                },
                /// <summary>
                /// The intensity of the CPU for grin/AE mining. Valid values are 0 to 12. Higher intensity may give better performance but more CPU usage. (default 6)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_cpu_intensity",
                    ShortName = "-intensity",
                    DefaultValue = "6"
                },
                /// <summary>
                /// Append the logs to the file <path>.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_logfile",
                    ShortName = "-logfile="
                },
                /// <summary>
                /// Disable the devfee but it also disables some optimizations.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "bminer_nofee",
                    ShortName = "-nofee"
                },
                /// <summary>
                /// Personalization string for equihash 144,5 based coins. Default: BgoldPoW. Valid values include BitcoinZ, Safecoin, ZelProof, etc. (default "BgoldPoW")
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_pers",
                    ShortName = "-pers",
                    DefaultValue = "BgoldPoW"
                }
            },
            TemperatureOptions = new List<MinerOption>{
                /// <summary>
                /// Hard limits of the temperature of the GPUs. Bminer slows down itself when the temperautres of the devices exceed the limit. (default 85)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_max_temp",
                    ShortName = "-max-temperature",
                    DefaultValue = "85"
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables{};
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };

        protected static MinerApiMaxTimeoutSetting _getApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout =  _defaultTimeout,
        };
        #endregion Internal settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "bminer.exe" });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
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
