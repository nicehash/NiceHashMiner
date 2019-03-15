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
        public string PluginUUID => "c9d482a8-3118-4976-ac83-a048489c5db5";

        public Version Version => new Version(1, 0);

        public string Name => "ZEnemy";

        public string Author => "luc1an24";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new ZEnemyBase();
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

        IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        {
            return new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.Skunk),
            };
        }
    }
}
