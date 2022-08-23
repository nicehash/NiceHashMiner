using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace NBMiner
{
    public partial class NBMinerPlugin
    {
        const ulong KAWPOW_RamLimit = (2UL << 30) + (2UL << 29) + (2UL << 28);
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // TODO fees are not just 2%
            DefaultFee = 2.0,
            AlgorithmFeesV2 = new Dictionary<string, double>
            {
                { $"{AlgorithmType.DaggerHashimoto}", 1.0 },
                { $"{AlgorithmType.Octopus}", 3.0 },
                { $"{AlgorithmType.EtcHash}", 1.0 },
            },
#if LHR_BUILD_ON
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.Autolykos),
                    }
                },
            }
#else
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.CuckooCycle),
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.KAWPOW) { NonDefaultRAMLimit = KAWPOW_RamLimit },
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.Octopus) {NonDefaultRAMLimit = 5UL << 30},
                        new SAS(AlgorithmType.Autolykos),
                        new SAS(AlgorithmType.EtcHash),
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW) {NonDefaultRAMLimit = KAWPOW_RamLimit },
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.Autolykos),
                        new SAS(AlgorithmType.EtcHash)//todo?
                        //new SAS(AlgorithmType.Octopus) {NonDefaultRAMLimit = 5UL << 30},
                    }
                }
            }
#endif
        };
    }
}
