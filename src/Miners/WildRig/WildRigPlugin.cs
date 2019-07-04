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
using System.Threading.Tasks;

namespace WildRig
{
    public class WildRigPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IDevicesCrossReference, IReBenchmarkChecker, IGetApiMaxTimeoutV2
    {
        public WildRigPlugin()
        {
            _pluginUUID = "7de4c3d8-0417-4d56-b6be-6c43820ca809";
        }
        public WildRigPlugin(string pluginUUID = "7de4c3d8-0417-4d56-b6be-6c43820ca809")
        {
            _pluginUUID = pluginUUID;
        }

        private readonly string _pluginUUID;

        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1,1);

        public string Name => "WildRig";

        public string Author => "domen.kirnkrefl@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "wildrig.exe" });
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--print-devices");
            var mappedDevs = DevicesListParser.ParseWildRigOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedIDs[uuid] = indexID;
            }
        }

        public IMiner CreateMiner()
        {
            return new WildRig(PluginUUID, _mappedIDs)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
            };
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn2(gpu)).Cast<AMDDevice>();

            var pcieId = 0;
            foreach (var gpu in amdGpus)
            {
                _mappedIDs[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                new Algorithm(PluginUUID, AlgorithmType.X16R)
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
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

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// strategy of feeding videocards with job (default: 0)
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_strategy",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--strategy=",
                    DefaultValue = "0"
                },
                /// <summary>
                /// list of launch config, intensity and worksize
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_launch",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-launch=",
                },
                /// <summary>
                /// affine GPU threads to a CPU
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_affinity",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-affinity=",
                },
                /// <summary>
                /// amount of threads per OpenCL device
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_threads",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-threads=",
                },
                /// <summary>
                /// log all output to a file
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_log",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ShortName = "-l",
                    LongName = "--log-file=",
                },
                /// <summary>
                /// print hashrate report every N seconds
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printTime",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--print-time=",
                },
                /// <summary>
                /// print hashrate for each videocard
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printFull",
                    Type = MinerOptionType.OptionIsParameter,
                    LongName = "--print-full",
                },
                /// <summary>
                /// print additional statistics
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printStatistics",
                    Type = MinerOptionType.OptionIsParameter,
                    LongName = "--print-statistics",
                },
                /// <summary>
                /// print debug information
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printDebug",
                    Type = MinerOptionType.OptionIsParameter,
                    LongName = "--print-debug",
                },
                /// <summary>
                /// donate level, default 2% (2 minutes in 100 minutes)
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_fee",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--donate-level=",
                    DefaultValue = "2",
                },
            }
        };
        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
        protected static MinerApiMaxTimeoutSetting _getApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout = _defaultTimeout,
        };
        #endregion Internal Settings

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            // nothing new
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
