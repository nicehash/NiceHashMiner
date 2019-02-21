using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPlugin.Interfaces;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common;
using System.IO;

namespace GMinerPlugin
{
    public class GMinerPlugin : IMinerPlugin, IInstalablePlugin
    {
        public string PluginUUID => "066745f3-6738-4b65-adbb-0d1e153ed873";

        public Version Version => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new GMiner();
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
        //   - CUDA 9.0+ // check the driver version for this one

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // we filter CUDA 5.0+
            var cudaGpus = devices.Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5).Select(dev => (CUDADevice)dev);
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, GetSupportedAlgorithms(gpu));
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(CUDADevice gpu) {
            var algorithms = new List<Algorithm>{};
            const ulong MinZHashMemory = 1879047230; // 1.75GB
            if (gpu.GpuRam > MinZHashMemory) {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.ZHash));
            }
            const ulong MinBeamMemory = 3113849695; // 2.9GB
            if (gpu.GpuRam > MinBeamMemory) {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.Beam));
            }
            const ulong MinGrinCuckaroo29Memory = 6012951136; // 5.6
            if (gpu.GpuRam > MinGrinCuckaroo29Memory) {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29));
            }

            return algorithms;
        }

        #region IInstalablePlugin
        public Task<bool> Install()
        {
            var minerExe = Path.Combine(Paths.MinerPlugins, PluginUUID, "miner.exe");
            // check if already installed
            if (File.Exists(minerExe))
            {

            }
        }

        public Task<bool> Uninstall()
        {
            throw new NotImplementedException();
        }
        #endregion IInstalablePlugin
    }
}
