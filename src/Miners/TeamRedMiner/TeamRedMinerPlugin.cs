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

namespace TeamRedMiner
{
    public class TeamRedMinerPlugin : IMinerPlugin, IInitInternals
    {
        public string PluginUUID => "189aaf80-4b23-11e9-a481-e144ccd86993";

        public Version Version => new Version(1, 1);

        public string Name => "TeamRedMiner";

        public string Author => "stanko@nicehash.com";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new TeamRedMiner(PluginUUID, AMDDevice.OpenCLPlatformID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        /// <summary>
        /// Get whether AMD device is GCN 4th gen or higher (400/500/Vega)
        /// </summary>
        internal static bool IsGcn4(AMDDevice dev)
        {
            if (dev.Name.Contains("Vega"))
                return true;
            if (dev.InfSection.ToLower().Contains("polaris"))
                return true;

            return false;
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // Get AMD GCN4+
            var amdGpus = devices
                .Where(dev => dev is AMDDevice gpu && IsGcn4(gpu))
                .Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, GetSupportedAlgorithms(gpu));
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm> {
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightV8),
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightR),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                //new Algorithm(PluginUUID, AlgorithmType.Lyra2Z),
            };
            return algorithms;
        }

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage{};
        #endregion Internal Settings
    }
}
