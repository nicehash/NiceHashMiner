using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CCMinerBase
{
    public class CCMinerPlugin : IMinerPlugin
    {
        public string PluginUUID => "5e387d39-8363-4e88-8d8e-c7dfc8a0ecd9";

        public Version Version => new Version(1, 0);

        public string Name => "ccminer-tpruvot-2.3.1_CUDA10";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            //return new CCMinerBase();
            return null;
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice).Select(dev => (CUDADevice)dev);
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                supported.Add(gpu, GetSupportedAlgorithms());
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms() {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.NeoScrypt), 
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv2), 
                new Algorithm(PluginUUID, AlgorithmType.Decred), 
                new Algorithm(PluginUUID, AlgorithmType.Lbry), 
                new Algorithm(PluginUUID, AlgorithmType.X11Gost), 
                new Algorithm(PluginUUID, AlgorithmType.Blake2s), 
                new Algorithm(PluginUUID, AlgorithmType.Sia), 
                new Algorithm(PluginUUID, AlgorithmType.Keccak), 
                new Algorithm(PluginUUID, AlgorithmType.Skunk), 
                new Algorithm(PluginUUID, AlgorithmType.Lyra2z), 
                new Algorithm(PluginUUID, AlgorithmType.X16R), 
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3), 
            };
        }
    }
}
