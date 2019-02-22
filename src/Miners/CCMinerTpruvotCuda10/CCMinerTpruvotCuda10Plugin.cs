using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CCMinerTpruvotCuda10
{
    public class CCMinerTpruvotCuda10Plugin : IMinerPlugin
    {
        public Version Version => new Version(1, 0);

        public string Name => "CCMinerTpruvotCuda10";

        public string PluginUUID => Shared.UUID;

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new CCMinerTpruvotCuda10Miner();
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // 410.48
            var minimumNVIDIADriver = new Version(410, 48);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minimumNVIDIADriver)
            {
                // TODO log
                return supported; // return emtpy
            }
            // SM3.+ and CUDA 10 drivers
            var cudaGpus = devices.Where(dev => dev is CUDADevice cudaDev && cudaDev.SM_major >= 3).Select(dev => (CUDADevice)dev);

            foreach (var gpu in cudaGpus)
            {
                supported.Add(gpu, GetSupportedAlgorithms());
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        {
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
