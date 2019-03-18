using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZEnemy
{
    class ZEnemyPlugin : IMinerPlugin
    {
        public Version Version => new Version(1, 0);

        public string Name => "ZEnemy";

        public string Author => "Domen Kirn Krefl";

        public string PluginUUID => "c9d482a8-3118-4976-ac83-a048489c5db5";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 6).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(411, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            yield return new Algorithm(PluginUUID, AlgorithmType.X16R);
            yield return new Algorithm(PluginUUID, AlgorithmType.Skunk);
        }

        public IMiner CreateMiner()
        {
            return new ZEnemy(PluginUUID);
        }

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }
    }
}
