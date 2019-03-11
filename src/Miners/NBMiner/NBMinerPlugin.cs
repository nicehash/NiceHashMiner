using System;
using System.Collections.Generic;
using System.Linq;
using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace NBMiner
{
    public class NBMinerPlugin : IMinerPlugin
    {
        public Version Version => new Version(1, 0);
        public string Name => "NBMiner";

        public string PluginUUID => "139935b0-7b7e-4016-855a-272ace00ce8a";

        private readonly Dictionary<int, int> _mappedCudaIDs = new Dictionary<int, int>();

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 377.xx
            var minDrivers = new Version(377, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major == 6 && gpu.SM_minor == 1)
                .Cast<CUDADevice>()
                .OrderBy(dev => dev.PCIeBusID);

            var pcieID = 0;
            foreach (var gpu in cudaGpus)
            {
                _mappedCudaIDs[gpu.ID] = pcieID++;
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            const ulong minGrin29Mem = 5UL << 30;
            const ulong minGrin31Mem = 8UL << 30;

            if (dev.GpuRam >= minGrin29Mem)
                yield return new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29);

            if (dev.GpuRam >= minGrin31Mem)
                yield return new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31);
        }

        public IMiner CreateMiner()
        {
            return new NBMiner(PluginUUID, _mappedCudaIDs);
        }

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }
    }
}
