using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace LolMinerBeam
{
    class LolMinerBeamPlugin : IMinerPlugin
    {
        public Version Version => new Version(1, 0);

        public string Name => "LolMinerBeam";

        public string Author => "Domen Kirn Krefl";

        public string PluginUUID => "a6c96e17-6fa7-418f-b75e-6631b3a5241e";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 2).Cast<CUDADevice>();
            var openCLGpus = devices.Where(dev => dev is AMDDevice).Cast<AMDDevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedCUDAAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            foreach (var gpu in openCLGpus)
            {
                var algos = GetSupportedAMDAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedCUDAAlgorithms(CUDADevice dev)
        {
            const ulong minBeamMem = 4UL << 30;

            if (dev.GpuRam >= minBeamMem)
                yield return new Algorithm(PluginUUID, AlgorithmType.Beam);
        }

        private IEnumerable<Algorithm> GetSupportedAMDAlgorithms(AMDDevice dev)
        {
            const ulong minBeamMem = 4UL << 30;

            if (dev.GpuRam >= minBeamMem)
                yield return new Algorithm(PluginUUID, AlgorithmType.Beam);
        }

        public IMiner CreateMiner()
        {
            return new LolMinerBeam(PluginUUID);
        }

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }
    }
}
