using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MinerPlugin;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace LolMinerBeam
{
    class LolMinerBeamPlugin : IMinerPlugin, IInitInternals
    {
        public Version Version => new Version(1, 1);

        public string Name => "LolMinerBeam";

        public string Author => "Domen Kirn Krefl";

        public string PluginUUID => "aafcf5d0-4bfb-11e9-a481-e144ccd86993";

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
            return new LolMinerBeam(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage { };
        #endregion Internal Settings
    }
}
