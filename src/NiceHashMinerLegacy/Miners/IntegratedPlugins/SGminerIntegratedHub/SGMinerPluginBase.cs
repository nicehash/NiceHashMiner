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
    abstract class SGMinerPluginBase : IMinerPlugin, IInitInternals, IntegratedPlugin, IBinaryPackageMissingFilesChecker
    {
        public bool Is3rdParty => false;

        public abstract string PluginUUID { get; }

        public abstract Version Version { get; }

        public abstract string Name { get; }

        public string Author => "stanko@nicehash.com";

        public IMiner CreateMiner()
        {
            return new SGminerIntegratedMiner(PluginUUID, AMDDevice.OpenCLPlatformID)
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

        // TODO make sure avemore has the same settings
        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            // we have same env vars for all miners now, check avemore env vars if they differ and use custom env vars instead of defaults
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_USE_SYNC_OBJECTS", "1"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
                {"GPU_FORCE_64BIT_PTR", "1"}
            },
        };

        protected static MinerOptionsPackage _minerOptionsPackage = SGMinerBase.DefaultMinerOptionsPackage;

        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };

        public abstract IEnumerable<string> CheckBinaryPackageMissingFiles();
    }
}
