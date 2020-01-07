using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace XMRig
{
    public partial class XMRigPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // default is 5 but we set it to 1
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.CPU,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.CryptoNightR),
                        new SAS(AlgorithmType.RandomXmonero),
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.CryptoNightR, "cn/r" },
                { AlgorithmType.RandomXmonero, "rx/0" },
            }
        };
    }
}
