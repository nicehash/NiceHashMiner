using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace SRBMiner
{
    public partial class SRBMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 0.85,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.Handshake),
                        new SAS(AlgorithmType.DaggerHashimoto) {NonDefaultRAMLimit = 5UL << 30}
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Handshake, "bl2bsha3" },
                { AlgorithmType.DaggerHashimoto, "ethash" }
            }
        };
    }
}
