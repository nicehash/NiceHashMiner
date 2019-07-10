using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.SgminerCommon;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    abstract class SGMinerPluginBase : IMinerPlugin, IInitInternals, IntegratedPlugin, IBinaryPackageMissingFilesChecker, IGetApiMaxTimeoutV2
    {
        public bool Is3rdParty => false;

        public abstract string PluginUUID { get; }

        public abstract Version Version { get; }

        public abstract string Name { get; }

        public string Author => "stanko@nicehash.com";

        public IMiner CreateMiner()
        {
            return new SGminerIntegratedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts,
                MinerBenchmarkTimeSettings = _minerBenchmarkTimeSettings
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            if (a.Device is AMDDevice aDev && b.Device is AMDDevice bDev && aDev.OpenCLPlatformID != bDev.OpenCLPlatformID)
            {
                // OpenCLPlatorm IDs must match
                return false;
            }
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public abstract Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);

        // TODO add ELP internal configs here 
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;

            var fileMinerReservedPorts = InternalConfigs.InitMinerReservedPorts(pluginRoot, _minerReservedApiPorts);
            if (fileMinerReservedPorts != null) _minerReservedApiPorts = fileMinerReservedPorts;

            var fileMinerBenchmarkTimeSetting = InternalConfigs.InitMinerBenchmarkTimeSettings(pluginRoot, _minerBenchmarkTimeSettings);
            if (fileMinerBenchmarkTimeSetting != null) _minerBenchmarkTimeSettings = fileMinerBenchmarkTimeSetting;
        }

        protected abstract MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables { get; set; }

        protected static MinerSystemEnvironmentVariables GetMinerSystemEnvironmentVariables(bool isAvemore)
        {
            if (isAvemore)
            {
                return new MinerSystemEnvironmentVariables
                {
                    DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
                    {
                        {"GPU_MAX_ALLOC_PERCENT", "100"},
                        {"GPU_USE_SYNC_OBJECTS", "1"},
                        {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                        {"GPU_MAX_HEAP_SIZE", "100"},
                        {"GPU_FORCE_64BIT_PTR", "0"}
                    },
                };
            }
            return new MinerSystemEnvironmentVariables
            {
                DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
                {
                    {"GPU_MAX_ALLOC_PERCENT", "100"},
                    {"GPU_USE_SYNC_OBJECTS", "1"},
                    {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                    {"GPU_MAX_HEAP_SIZE", "100"},
                    {"GPU_FORCE_64BIT_PTR", "1"}
                },
            };
        }



        protected static MinerOptionsPackage _minerOptionsPackage = SGMinerBase.DefaultMinerOptionsPackage;

        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };

        protected static MinerApiMaxTimeoutSetting _getApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout =  _defaultTimeout,
        };
        protected static MinerBenchmarkTimeSettings _minerBenchmarkTimeSettings = new MinerBenchmarkTimeSettings { };

        public abstract IEnumerable<string> CheckBinaryPackageMissingFiles();
        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }

        #region IGetApiMaxTimeoutV2
        public bool IsGetApiMaxTimeoutEnabled
        {
            get
            {
                if (_getApiMaxTimeoutConfig?.UseUserSettings ?? false) return _getApiMaxTimeoutConfig.Enabled;
                return true;
            }
        }


        protected static TimeSpan _defaultTimeout = new TimeSpan(0, 5, 0);
        public TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return MinerApiMaxTimeoutSetting.ParseMaxTimeout(_defaultTimeout, _getApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2

    }
}
