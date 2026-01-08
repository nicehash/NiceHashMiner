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
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit =  (4UL << 30) , Enabled = false },
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = false},
                        new SAS(AlgorithmType.Autolykos) { Enabled = false},
                        new SAS(AlgorithmType.EtcHash) {NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false }
                    }
                },
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){ NonDefaultRAMLimit =  (4UL << 30) , Enabled = false },
                        new SAS(AlgorithmType.Octopus) { Enabled = false, NonDefaultRAMLimit = (5UL << 30) + (4UL << 29)},
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = false},
                        new SAS(AlgorithmType.Autolykos) { Enabled = false},
                        new SAS(AlgorithmType.EtcHash) {NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false }
                    }
                },
                {
                    DeviceType.CPU,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.VerusHash)
                    }
                }
            }
        };
    }
}
