using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace MP.Joker
{
    public partial class JokerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            UseUserSettings = true, // since we want the user to edit these files make it easier
            // fixed fee
            DefaultFee = 1.0,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.DaggerHashimoto, 1 }
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.CPU,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.RandomXmonero),
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto),
                    }
                },
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto),
                    }
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.DaggerHashimoto, "ethash" },
                { AlgorithmType.RandomXmonero, "xmr" },
            }
        };
    }
}
