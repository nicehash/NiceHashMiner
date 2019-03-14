using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoDredge
{
    class CryptoDredgePlugin : IMinerPlugin
    {
        public Version Version => new Version(1, 0);
        public string Name => "CryptoDredge";

        public string Author => "Domen Kirn Krefl";

        public string PluginUUID => "c5bea9fd-5660-4ccb-9f0e-a3f500d228c8";



        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var minDrivers = new Version(397, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            const ulong minMem = 2UL << 30;
            const ulong minMTPMem = 5UL << 30;

            if (dev.GpuRam >= minMem)
            {
                yield return new Algorithm(PluginUUID, AlgorithmType.CryptoNightHeavy);
                yield return new Algorithm(PluginUUID, AlgorithmType.CryptoNightV8);
                yield return new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3);
                yield return new Algorithm(PluginUUID, AlgorithmType.NeoScrypt);
                yield return new Algorithm(PluginUUID, AlgorithmType.X16R);
            }
            if(dev.GpuRam >= minMTPMem)
            {
                yield return new Algorithm(PluginUUID, AlgorithmType.MTP);
            }
        }

        public IMiner CreateMiner()
        {
            return new CryptoDredge(PluginUUID);
        }

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }
    }
}
