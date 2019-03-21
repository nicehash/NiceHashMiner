using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.Configs;
using System.IO;
using NiceHashMinerLegacy.Common;
using MinerPluginToolkitV1.SgminerCommon;
using MinerPluginToolkitV1.ExtraLaunchParameters;

namespace SGMinerHub
{
    // SGMinerHubPlugin combines multiple miners
    public class SGMinerHubPlugin : IMinerPlugin, IInitInternals
    {
        public string PluginUUID => "1b246660-4bcd-11e9-87d3-6b57d758e2c6";

        public Version Version => new Version(1, 0);

        public string Name => "SGMinerHub";

        public string Author => "stanko@nicehash.com";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new SGMiner(PluginUUID, AMDDevice.OpenCLPlatformID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var amdGpus = devices
                .Where(dev => dev is AMDDevice)
                .Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, GetSupportedAlgorithms(gpu));
            }

            return supported;
        }

        // TODO check if We should add NeoScrypt
        IReadOnlyList<Algorithm> GetSupportedAlgorithms(AMDDevice gpu)
        {
            const string RemDis = " --remove-disabled";
            const string DefaultParam = " --keccak-unroll 0 --hamsi-expand-big 4 --remove-disabled ";

            var algorithms = new List<Algorithm> {
                new Algorithm(PluginUUID, AlgorithmType.Keccak)
                {
                    ExtraLaunchParameters = DefaultParam + "--intensity 15"
                },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto)
                {
                    ExtraLaunchParameters = RemDis + " --xintensity 512 -w 192 -g 1"
                },
                new Algorithm(PluginUUID, AlgorithmType.X16R)
                {
                    ExtraLaunchParameters = "-X 256"
                },
            };
            return algorithms;
        }


        // TODO add ELP internal configs here 
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        private static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
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

        private static MinerOptionsPackage _minerOptionsPackage = SGMinerBase.DefaultMinerOptionsPackage;
    }
}
