using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.SgminerCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    abstract class CCMinersPluginBase : IMinerPlugin, IInitInternals, IntegratedPlugin, IBinaryPackageMissingFilesChecker, IGetApiMaxTimeoutV2
    {
        public bool Is3rdParty => false;

        protected abstract string DirPath { get; }

        public abstract string PluginUUID { get; }

        public abstract Version Version { get; }

        public abstract string Name { get; }

        public string Author => "stanko@nicehash.com";

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }
        
        public IMiner CreateMiner()
        {
            return new CCMinerIntegratedMiner(PluginUUID, DirPath)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
            };
        }

        public abstract Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);

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

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU threads per call 8-25 (2^N + F, default: 0=auto). Decimals and multiple values are allowed for fine tuning
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ccminertpruvot_intensity",
                    ShortName = "-i",
                    LongName = "--intensity=",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// number of miner threads (default: number of nVidia GPUs in your system)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_threads",
                    ShortName = "-t",
                    LongName = "--threads=",
                }, 
                /// <summary>
                /// Set device threads scheduling mode (default: auto)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ccminertpruvot_cuda_schedule",
                    ShortName = "--cuda-schedule",
                },
                /// <summary>
                /// set process priority (default: 0 idle, 2 normal to 5 highest)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "0",
                },
                /// <summary>
                /// set process affinity to specific cpu core(s) mask
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_affinity",
                    ShortName = "--cpu-affinity",
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

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            if (PluginUUID != "CCMinerTpruvot")
            {
                return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "ccminer.exe" });
            }
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "ccminer-x64.exe" });
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
