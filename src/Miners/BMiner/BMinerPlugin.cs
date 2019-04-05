using MinerPlugin;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BMiner
{
    public class BMinerPlugin : IMinerPlugin, IInitInternals
    {
        public BMinerPlugin(string pluginUUID = "92a7fd10-498d-11e9-87d3-6b57d758e2c6")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 2);
        public string Name => "BMiner";

        public string Author => "Domen Kirn Krefl";

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
            // CUDA 9.2+ driver 397.44
            var mininumRequiredDriver = new Version(397, 44);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS >= mininumRequiredDriver)
            {
                var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();
                foreach (var gpu in cudaGpus)
                {
                    var algos = GetCUDASupportedAlgorithms(gpu).ToList();
                    if (algos.Count > 0) supported.Add(gpu, algos);
                }
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetCUDASupportedAlgorithms(CUDADevice dev)
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

        private IEnumerable<Algorithm> GetAMDSupportedAlgorithms(AMDDevice dev)
        {
            const ulong minBeamMem = 3113849695;
            if (dev.GpuRam >= minBeamMem)
                yield return new Algorithm(PluginUUID, AlgorithmType.Beam);
        }

        public IMiner CreateMiner()
        {
            return new BMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        #region Internal settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// The intensity of the CPU for grin/AE mining. Valid values are 0 to 12. Higher intensity may give better performance but more CPU usage. (default 6)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_cpu_intensity",
                    ShortName = "-intensity",
                    DefaultValue = "6",
                },
                /// <summary>
                /// Hard limits of the temperature of the GPUs. Bminer slows down itself when the temperautres of the devices exceed the limit. (default 85)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_max_temp",
                    ShortName = "-max-temperature",
                    DefaultValue = "85",
                },
                /// <summary>
                /// Disable the devfee but it also disables some optimizations.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "bminer_nofee",
                    ShortName = "-nofee",
                }
            }
        };
        #endregion Internal settings
    }
}
