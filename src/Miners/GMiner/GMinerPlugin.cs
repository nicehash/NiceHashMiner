using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common;
using System.IO;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;

namespace GMinerPlugin
{
    public class GMinerPlugin : IMinerPlugin, IInitInternals
    {
        public string PluginUUID => "5def7740-4bfb-11e9-a481-e144ccd86993";

        public Version Version => new Version(1, 1);

        public string Name => "GMinerCuda9.0+";

        public string Author => "stanko@nicehash.com";

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new GMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }


        // Supported algoritms:
        //   - Cuckaroo29/Cuckatoo31 (Grin)
        //   - Cuckoo29 (Aeternity)
        //   - Equihash 96,5 (MinexCoin)
        //   - Equihash 144,5 (Bitcoin Gold, BitcoinZ, SnowGem, SafeCoin, Litecoin Z) // ZHash
        //   - Equihash 150,5 (BEAM)
        //   - Equihash 192,7 (Zero, Genesis)
        //   - Equihash 210,9 (Aion)

        // Requirements:
        //   - CUDA compute compability 5.0+ #1
        //   - Cuckaroo29 ~ 5.6GB VRAM
        //   - Cuckatoo31 ~ 7.4GB VRAM
        //   - Cuckoo29 ~ 5.6GB VRAM
        //   - Equihash 96,5 ~0.75GB VRAM
        //   - Equihash 144,5 ~1.75GB VRAM
        //   - Equihash 150,5 ~2.9GB VRAM
        //   - Equihash 192,7 ~2.75GB VRAM
        //   - Equihash 210,9 ~1GB VRAM
        //   - CUDA 9.0+ 
        internal static bool IsGcn4(AMDDevice dev)
        {
            if (dev.Name.Contains("Vega"))
                return true;
            if (dev.InfSection.ToLower().Contains("polaris"))
                return true;

            return false;
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && IsGcn4(gpu)).Cast<AMDDevice>();
            foreach (var gpu in amdGpus)
            {
                var algorithms = GetAMDSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            //CUDA 9.0+: minimum drivers 384.xx
            var minDrivers = new Version(384, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            // we filter CUDA SM5.0+ and order them by PCIe IDs
            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Select(dev => (CUDADevice)dev)
                .OrderBy(dev => dev.PCIeBusID);
            var pcieId = 0; // GMiner takes CUDA devices by 
            foreach (var gpu in cudaGpus)
            {
                Shared.MappedCudaIds[gpu.ID] = pcieId;
                ++pcieId;
                var algorithms = GetCUDASupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, GetCUDASupportedAlgorithms(gpu));
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetCUDASupportedAlgorithms(CUDADevice gpu) {
            var algorithms = new List<Algorithm>{};
            const ulong MinZHashMemory = 1879047230; // 1.75GB
            if (gpu.GpuRam > MinZHashMemory) {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.ZHash));
            }
            const ulong MinBeamMemory = 3113849695; // 2.9GB
            if (gpu.GpuRam > MinBeamMemory) {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.Beam));
            }
            const ulong MinGrinCuckaroo29Memory = 6012951136; // 5.6GB
            if (gpu.GpuRam > MinGrinCuckaroo29Memory) {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29));
            }

            return algorithms;
        }

        IReadOnlyList<Algorithm> GetAMDSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm> { };
            const ulong MinBeamMemory = 3113849695; // 2.9GB
            if (gpu.GpuRam > MinBeamMemory)
            {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.Beam));
            }
            return algorithms;
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
