using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace NanoMiner
{
    public partial class NanoMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 2.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
#if LHR_BUILD_ON
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto)
                    }
                }
            },
#else
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.Autolykos),
                        new SAS(AlgorithmType.EtcHash)
                    }
                },
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){ NonDefaultRAMLimit = (2UL << 30) + (2UL << 29) + (2UL << 28)},
                        new SAS(AlgorithmType.Octopus),
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.Autolykos),
                        new SAS(AlgorithmType.EtcHash)
                    }
                }
            },
#endif
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.KAWPOW, "Kawpow" },
                { AlgorithmType.Octopus, "Octopus" },
                { AlgorithmType.DaggerHashimoto, "Ethash" },
                { AlgorithmType.Autolykos, "autolykos" },
                { AlgorithmType.EtcHash, "Etchash" }
            }
        };
    }
}
