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

namespace EWBF
{
    public class EwbfPlugin : IMinerPlugin, IInitInternals
    {
        public string PluginUUID => "3e627d60-4bfa-11e9-a481-e144ccd86993";

        public Version Version => new Version(1, 1);

        public string Name => "Ewbf";

        public string Author => "stanko@nicehash.com";

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new EwbfMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            //CUDA 9.1+: minimum drivers 391.29
            var minDrivers = new Version(391, 29);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            // we filter CUDA SM5.0+
            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, GetSupportedAlgorithms(gpu));
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm> { };
            // on btctalk ~1.63GB vram
            const ulong MinZHashMemory = 1879047230; // 1.75GB
            if (gpu.GpuRam > MinZHashMemory)
            {
                algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.ZHash));
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

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Personalization for equihash, string 8 characters
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_personalization_equihash",
                    ShortName = "--pers"
                },
                /// <summary>
                /// The developer fee in percent allowed decimals for example 0, 1, 2.5, 1.5 etc.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_developer_fee",
                    ShortName = "--fee",
                }
            }
        };
        #endregion Internal Settings
    }
}
