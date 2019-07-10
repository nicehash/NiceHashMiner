using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;

namespace LolMinerBeam
{
    class LolMinerBeamPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2
    {
        public Version Version => new Version(1, 6);

        public string Name => "LolMinerBeam";

        public string Author => "domen.kirnkrefl@nicehash.com";

        public string PluginUUID => "435f0820-7237-11e9-b20c-f9f12eb6d835";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            //CUDA 9.0+: minimum drivers 384.xx
            var minDrivers = new Version(384, 0);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;

            var gpus = devices
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported))
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            var pcieId = 0; // GMiner sortes devices by PCIe
            foreach (var gpu in gpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu as BaseDevice, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(BaseDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 2;
            return isSupported && isDriverSupported;
        }


        private IEnumerable<Algorithm> GetSupportedAlgorithms(IGpuDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.Beam),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public IMiner CreateMiner()
        {
            return new LolMinerBeam(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts,
                MinerBenchmarkTimeSettings = _minerBenchmarkTimeSettings
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;

            var fileMinerReservedPorts = InternalConfigs.InitMinerReservedPorts(pluginRoot, _minerReservedApiPorts);
            if (fileMinerReservedPorts != null) _minerReservedApiPorts = fileMinerReservedPorts;

            var fileMinerApiMaxTimeoutSetting = InternalConfigs.InitMinerApiMaxTimeoutSetting(pluginRoot, _getApiMaxTimeoutConfig);
            if (fileMinerApiMaxTimeoutSetting != null) _getApiMaxTimeoutConfig = fileMinerApiMaxTimeoutSetting;

            var fileMinerBenchmarkTimeSetting = InternalConfigs.InitMinerBenchmarkTimeSettings(pluginRoot, _minerBenchmarkTimeSettings);
            if (fileMinerBenchmarkTimeSetting != null) _minerBenchmarkTimeSettings = fileMinerBenchmarkTimeSetting;
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// When set to 1 this parameter turns on the usage of assembly tuned kernels.
                /// At the moment lolMiner 0.7.1 only features assembly tuned kernels for mining Beam on RX470 / 480 / 570 / 580, Vega 56 and 64 and Radeon VII
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_activateBinaryKernels",
                    ShortName = "--asm",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Enables (1) or Disables (0) to make the miner write its text output to a log file.
                /// The file will be located in the “logs” directory at the miner location and will be named by the date and time the miner started.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableLogs",
                    ShortName = "--logs",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This two parameters control the length between two statistics show. The longer interval statistics is shown with a blue color, the shorter only black and while.
                /// Setting an interval length of 0 will disable the corresponding statistics output.
                /// Note: disabling the short statistics output will also disable the shortaccept option (see below).
                /// Also the intervals are used for updating the API output.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_longStats",
                    ShortName = "--longstats",
                    DefaultValue = "300"
                },
                /// <summary>
                /// This two parameters control the length between two statistics show. The longer interval statistics is shown with a blue color, the shorter only black and while.
                /// Setting an interval length of 0 will disable the corresponding statistics output.
                /// Note: disabling the short statistics output will also disable the shortaccept option (see below).
                /// Also the intervals are used for updating the API output.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_shortStats",
                    ShortName = "--shortstats",
                    DefaultValue = "30"
                },
                /// <summary>
                /// When setting this parameter to 1, lolMiner will replace the “submitting share / share accepted” message pair by * symbols at the short statistics interval output.
                /// Every star stands for an accepted share.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_compactNotification",
                    ShortName = "--shortaccept",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Setting this parameter to 1 will activate the current daytime to be printed in the command-line console at each statistics output.
                /// This is true for command line as well as the log file when used.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableTimestamps",
                    ShortName = "--timeprint",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This parameter can be used to fix the sol/s output of a GPU to a fixed number of digits after the decimal delimiter.
                /// For example “DIGITS” : 0 will chop of all digits after the decimal delimiter. 
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_decimalDigits",
                    ShortName = "--digits",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This parameter can be used to set a new location for the kernel directory. Absolute path are allowed, so you can freely place it when needed.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_kernelsLocation",
                    ShortName = "--kernelsdir",
                    DefaultValue = "./kernels"
                }
            }
        };
        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
        protected static MinerApiMaxTimeoutSetting _getApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout =  _defaultTimeout,
        };
        protected static MinerBenchmarkTimeSettings _minerBenchmarkTimeSettings = new MinerBenchmarkTimeSettings { };
        #endregion Internal Settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "lolMiner.exe" });
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
