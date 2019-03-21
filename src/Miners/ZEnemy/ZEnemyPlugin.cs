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

namespace ZEnemy
{
    class ZEnemyPlugin : IMinerPlugin, IInitInternals
    {
        public Version Version => new Version(1, 1);

        public string Name => "ZEnemy";

        public string Author => "Domen Kirn Krefl";

        public string PluginUUID => "c9d482a8-3118-4976-ac83-a048489c5db5";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 6).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(411, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            yield return new Algorithm(PluginUUID, AlgorithmType.X16R);
            yield return new Algorithm(PluginUUID, AlgorithmType.Skunk);
        }

        public IMiner CreateMiner()
        {
            return new ZEnemy(PluginUUID)
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
            if (fileMinerOptionsPackage != null)
                _minerOptionsPackage = fileMinerOptionsPackage;
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU intensity 8.0-31.0, decimals allowed (default: 19)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "zenemy_intensity",
                    ShortName = "-i",
                    LongName = "--intensity=",
                    DefaultValue = "19",
                    Delimiter = ","
                },
                /// <summary>
                /// set process priority (default: 3) 0 idle, 2 normal to 5 highest
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "zenemy_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "3"
                },
                //TODO WARNING this functionality can overlap with already implemented one!!!
                /// <summary>
                /// set process affinity to cpu core(s), mask 0x3 for cores 0 and 1
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "zenemy_affinity",
                    ShortName = "--cpu-affinity",
                }
            }
        };
        #endregion Internal Settings
    }
}
