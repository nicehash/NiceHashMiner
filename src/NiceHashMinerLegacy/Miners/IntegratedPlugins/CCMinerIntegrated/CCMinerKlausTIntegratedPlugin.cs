using System;
using System.Linq;
using System.Collections.Generic;
using MinerPluginToolkitV1;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class CCMinerKlausTIntegratedPlugin : CCMinersPluginBase
    {
        public override string PluginUUID => "CCMinerKlausT";

        public override Version Version => new Version(1,0);

        public override string Name => "CCMinerKlausT";

        protected override string DirPath => "ccminer_klaust";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var reqCudaVer = Checkers.CudaVersion.CUDA_9_1_85;
            var isCompatible = Checkers.IsCudaCompatibleDriver(reqCudaVer, CUDADevice.INSTALLED_NVIDIA_DRIVERS);
            if (!isCompatible) return supported; // return emtpy

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = new List<Algorithm> {
                    new Algorithm(PluginUUID, AlgorithmType.NeoScrypt)
                };
                supported.Add(gpu, algorithms);
            }

            return supported;
        }
    }
}
