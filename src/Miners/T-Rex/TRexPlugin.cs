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

namespace TRex
{
    public class TRexPlugin : IMinerPlugin, IInitInternals
    {
        public Version Version => new Version(1, 1);

        public string Name => "TRex";

        public string Author => "Domen Kirn Krefl";

        public string PluginUUID => "078bf411-9231-4ff1-8d73-7e65e2c1f67a";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();
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
            yield return new Algorithm(PluginUUID, AlgorithmType.Lyra2Z);
            yield return new Algorithm(PluginUUID, AlgorithmType.Skunk);
            yield return new Algorithm(PluginUUID, AlgorithmType.X16R);
        }

        public IMiner CreateMiner()
        {
            return new TRex(PluginUUID)
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
            var pluginRootIntenrals = Path.Combine(pluginRoot, "internals");
            var minerOptionsPackagePath = Path.Combine(pluginRootIntenrals, "MinerOptionsPackage.json");
            var fileMinerOptionsPackage = InternalConfigs.ReadFileSettings<MinerOptionsPackage>(minerOptionsPackagePath);
            if (fileMinerOptionsPackage != null && fileMinerOptionsPackage.UseUserSettings)
            {
                _minerOptionsPackage = fileMinerOptionsPackage;
            }
            else
            {
                InternalConfigs.WriteFileSettings(minerOptionsPackagePath, _minerOptionsPackage);
            }
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage{
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU intensity 8-25 (default: auto).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    DefaultValue = "auto"
                },

                /// <summary>
                /// Set process priority (default: 2) 0 idle, 2 normal to 5 highest.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "2"
                },
                
            }
        };
        #endregion Internal Settings
    }
}
