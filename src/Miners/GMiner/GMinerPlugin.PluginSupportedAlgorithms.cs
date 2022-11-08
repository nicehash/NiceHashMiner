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
        const ulong KAWPOW_RamLimit = (2UL << 30) + (2UL << 29) + (2UL << 28);
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
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = false },
                        new SAS(AlgorithmType.EtcHash) {NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { NonDefaultRAMLimit = KAWPOW_RamLimit, Enabled = false },
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.KHeavyHash),
                        // new SAS(AlgorithmType.CuckooCycle),
                        new SAS(AlgorithmType.ZelHash) { Enabled = false },
                        // new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.ZHash) { Enabled = false }
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = false },
                        new SAS(AlgorithmType.EtcHash) {NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { NonDefaultRAMLimit = KAWPOW_RamLimit, Enabled = false },
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.KHeavyHash),
                        // new SAS(AlgorithmType.CuckooCycle),
                        new SAS(AlgorithmType.ZelHash) { Enabled = false },
                        // new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.ZHash) { Enabled = false }
                    }
                }
            }
        };
    }
}
