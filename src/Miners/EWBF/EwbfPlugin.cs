using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPlugin.Interfaces;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common;
using System.IO;

namespace EWBF
{
    public class EwbfPlugin : IMinerPlugin
    {
        public string PluginUUID => "d836f10f-9f33-400a-8aee-5927144e75d5";

        public Version Version => new Version(1, 0);

        public string Name => "Ewbf";

        public string Author => "stanko@nicehash.com";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new EwbfMiner(PluginUUID);
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            //CUDA 9.1+: minimum drivers 391.29
            var minDrivers = new Version(391, 29);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            // we filter CUDA SM5.0+
            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, GetSupportedAlgorithms(gpu));
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm> { };
            // on btctalk ~1.63GB vram
            const ulong MinZHashMemory = 1879047230; // 1.75GB
            if (gpu.GpuRam > MinZHashMemory)
            {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.ZHash));
            }

            return algorithms;
        }
    }
}
