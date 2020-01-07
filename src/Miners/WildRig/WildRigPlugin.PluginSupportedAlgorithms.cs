using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace WildRig
{
    public partial class WildRigPlugin
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
                        new SAS(AlgorithmType.Lyra2REv3),
                        new SAS(AlgorithmType.X16R),
                        new SAS(AlgorithmType.X16Rv2),
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Lyra2REv3, "lyra2v3" },
                { AlgorithmType.X16R, "x16r" },
                { AlgorithmType.X16Rv2, "x16rv2" },
            }
        };
    }
}
