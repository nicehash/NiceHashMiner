using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP.GMiner
{
    public partial class GMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // TODO fees are not just 2%
            DefaultFee = 2.0,
            AlgorithmFeesV2 = new Dictionary<string, double>
            {
                { $"{AlgorithmType.DaggerHashimoto}", 1.0 },
                { $"{AlgorithmType.EtcHash}", 1.0 },
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto){Enabled = false},
                        new SAS(AlgorithmType.EtcHash) {NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { NonDefaultRAMLimit =  (4UL << 30)  },
                        new SAS(AlgorithmType.Autolykos),
                        new SAS(AlgorithmType.CuckooCycle){Enabled = false},
                        new SAS(AlgorithmType.ZelHash){Enabled = false},
                        new SAS(AlgorithmType.ZHash){Enabled = false},
                        new SAS(AlgorithmType.Octopus) {NonDefaultRAMLimit = (5UL << 30) + (4UL << 29)},
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto){Enabled = false},
                        new SAS(AlgorithmType.EtcHash) {NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { NonDefaultRAMLimit =  (4UL << 30)  },
                        new SAS(AlgorithmType.Autolykos),
                        new SAS(AlgorithmType.CuckooCycle){Enabled = false},
                        new SAS(AlgorithmType.ZelHash){Enabled = false},
                        new SAS(AlgorithmType.ZHash){Enabled = false},
                        new SAS(AlgorithmType.Octopus) {NonDefaultRAMLimit = (5UL << 30) + (4UL << 29)},
                    }
                }
            }
        };
    }
}
