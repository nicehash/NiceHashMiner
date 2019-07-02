using MinerPlugin;
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
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    abstract class SGMinerPluginBase : IMinerPlugin, IInitInternals, IntegratedPlugin, IBinaryPackageMissingFilesChecker, IGetApiMaxTimeout
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
                MinerReservedApiPorts = _minerReservedApiPorts
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

        public abstract IEnumerable<string> CheckBinaryPackageMissingFiles();

        public TimeSpan GetApiMaxTimeout()
        {
            return new TimeSpan(0, 5, 0);
        }

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
