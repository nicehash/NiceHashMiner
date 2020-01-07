using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace MiniZ
{
    public partial class MiniZPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fixed dev fee
            DefaultFee = 2.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.ZHash),
                        new SAS(AlgorithmType.BeamV2),
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.ZHash, "144,5" },
                { AlgorithmType.BeamV2, "150,5,3" },
            }
        };
    }
}
