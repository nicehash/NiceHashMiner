using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace T_RexBase
{
    public class T_RexPlugin : IMinerPlugin
    {
        public string PluginUUID => "078bf411-9231-4ff1-8d73-7e65e2c1f67a";

        public Version Version => new Version(1, 0);

        public string Name => "T-Rex";

        public string Author => "luc1an24";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new T_RexBase();
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
                new Algorithm(PluginUUID, AlgorithmType.Lyra2Z),
                new Algorithm(PluginUUID, AlgorithmType.Skunk),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
            };
        }
    }
}
