using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace Excavator
{
    public partial class ExcavatorPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 0.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.EtcHash),
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { Enabled = false }
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.EtcHash),
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { Enabled = false }
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.DaggerHashimoto, "daggerhashimoto" },
                { AlgorithmType.EtcHash, "etchash" },
                { AlgorithmType.Autolykos, "autolykos" },
                { AlgorithmType.KAWPOW, "kawpow" }
            }
        };
    }
}
