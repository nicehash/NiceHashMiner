using MinerPlugin;
using MinerPluginToolkitV1;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class ClaymoreIntegratedPlugin : ClaymorePluginBase
    {
        const string _pluginUUIDName = "Claymore";

        public override string PluginUUID => _pluginUUIDName;

        public override Version Version => new Version(1, 0);

        public override string Name => _pluginUUIDName;

        public override IMiner CreateMiner()
        {
            return new ClaymoreIntegratedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var supportedGpus = devices.Where(dev => dev is AMDDevice amd && Checkers.IsGcn4(amd) || dev is CUDADevice cuda && cuda.SM_major >= 5)
                .Cast<IGpuDevice>()
                .ToList();

            foreach (var gpu in supportedGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }
    }
}
