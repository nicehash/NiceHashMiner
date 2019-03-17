using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TeamRedMiner
{
    public class TeamRedMinerPlugin : IMinerPlugin
    {
        public string PluginUUID => "c9b45549-2392-449a-bad5-f90f7df16e96";

        public Version Version => new Version(1, 0);

        public string Name => "TeamRedMiner";

        public string Author => "stanko@nicehash.com";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new TeamRedMiner(PluginUUID, AMDDevice.OpenCLPlatformID);
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
    }
}
