using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMiner
{
    class BMinerPlugin : IMinerPlugin
    {
        public Version Version => new Version(1, 0);
        public string Name => "BMiner";

        public string Author => "Domen Kirn Krefl";

        public string PluginUUID => "c7e2060d-aebf-44bb-a7e5-dcec5de8243b";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();
            //todo check driver version
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            const ulong minZhashMem = 1879047230;
            const ulong minBeamMem = 3113849695;
            const ulong minGrin29Mem = 6012951136;
            const ulong minGrin31Mem = 9UL << 30;

            if (dev.GpuRam >= minZhashMem)
                yield return new Algorithm(PluginUUID, AlgorithmType.ZHash);

            if (dev.GpuRam >= minBeamMem)
                yield return new Algorithm(PluginUUID, AlgorithmType.Beam);

            if (dev.GpuRam >= minGrin29Mem)
                yield return new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29);

            if (dev.GpuRam >= minGrin31Mem)
                yield return new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31);
        }

        public IMiner CreateMiner()
        {
            return new BMiner(PluginUUID);
        }

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }
    }
}
