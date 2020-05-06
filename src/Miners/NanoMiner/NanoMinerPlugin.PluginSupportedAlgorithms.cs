using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

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
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.RandomXmonero){ Enabled=false}
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.RandomXmonero){ Enabled=false},
                        new SAS(AlgorithmType.KAWPOW)
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.RandomXmonero, "RandomX" },
                { AlgorithmType.KAWPOW, "Kawpow" },
            }
        };
    }
}
