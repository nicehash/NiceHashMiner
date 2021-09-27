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
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.DaggerHashimoto)
                    }
                },
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.Octopus),
                        new SAS(AlgorithmType.DaggerHashimoto)
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.KAWPOW, "Kawpow" },
                { AlgorithmType.Octopus, "Octopus" },
                { AlgorithmType.DaggerHashimoto, "Ethash" }
            }
        };
    }
}
