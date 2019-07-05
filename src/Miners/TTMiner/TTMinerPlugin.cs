using System;
using System.Collections.Generic;
using System.Linq;
using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.Configs;
using System.IO;
using NiceHashMinerLegacy.Common;
using MinerPluginToolkitV1;

namespace TTMiner
{
    public class TTMinerPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2
    {
        public TTMinerPlugin()
        {
            _pluginUUID = "f1945a30-7237-11e9-b20c-f9f12eb6d835";
        }
        public TTMinerPlugin(string pluginUUID = "f1945a30-7237-11e9-b20c-f9f12eb6d835")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 3);
        public string Name => "TTMiner";
        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 398.26
            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.MTP) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
            };
        }

        public IMiner CreateMiner()
        {
            return new TTMiner(PluginUUID)
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
        }

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Comma or space separated list of intensities that should be used mining.
			    /// First value for first GPU and so on. A single value sets the same intensity to all GPUs. A value of -1 uses the default intensity of the miner.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_intensity",
                    ShortName = "-i",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid. Same as intensity (-i) just defines the size for the grid directly.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_intensity_grid",
                    ShortName = "-ig",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid-size. This will give you more and finer control about the gridsize.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_grid_size",
                    ShortName = "-gs",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// Enable logging of screen output and additional information, the file is created in the folder 'Logs'.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ttminer_log",
                    ShortName = "-log",
                },
                /// <summary>
                /// This option set the process priority for TT-Miner to a different level:
                /// 1 low
                /// 2 below normal
                /// 3 normal
                /// 4 above normal
                /// 5 high
                /// Default: -PP 3
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ttminer_processPriority",
                    ShortName = "-PP",
                    DefaultValue = "3"
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nvml.dll", "nvrtc-builtins64_100.dll", "nvrtc-builtins64_101.dll",
                "nvrtc-builtins64_92.dll", "nvrtc64_100_0.dll", "nvrtc64_101_0.dll", "nvrtc64_92.dll", "SubSystemDll.dll", "TT-Miner.exe", @"Algos\AlgoEthash-C100.dll",
                @"Algos\AlgoEthash-C92.dll", @"Algos\AlgoEthash.dll", @"Algos\AlgoLyra2Rev3-C100.dll", @"Algos\AlgoLyra2Rev3-C92.dll", @"Algos\AlgoLyra2Rev3.dll", @"Algos\AlgoMTP-C100.dll",
                @"Algos\AlgoMTP-C92.dll", @"Algos\AlgoMTP.dll", @"Algos\AlgoMyrGr-C100.dll", @"Algos\AlgoMyrGr-C92.dll", @"Algos\AlgoMyrGr.dll", @"Algos\AlgoProgPoW-C100.dll",
                @"Algos\AlgoProgPoW-C92.dll", @"Algos\AlgoProgPoW.dll", @"Algos\AlgoUbqhash-C100.dll", @"Algos\AlgoUbqhash-C92.dll", @"Algos\AlgoUbqhash.dll"
            });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }

        #region IGetApiMaxTimeoutV2
        public bool IsGetApiMaxTimeoutEnabled => MinerApiMaxTimeoutSetting.ParseIsEnabled(true, _getApiMaxTimeoutConfig);

        protected static TimeSpan _defaultTimeout = new TimeSpan(0, 3, 0);
        public TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return MinerApiMaxTimeoutSetting.ParseMaxTimeout(_defaultTimeout, _getApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2
    }
}
