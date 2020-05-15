using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace WildRig
{
    public partial class WildRigPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.Lyra2REv3),
                        new SAS(AlgorithmType.X16R) { Enabled=false },
                        new SAS(AlgorithmType.X16Rv2),
                        //new SAS(AlgorithmType.KAWPOW) // doesn't seem to be working
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Lyra2REv3, "lyra2v3" },
                { AlgorithmType.X16R, "x16r" },
                { AlgorithmType.X16Rv2, "x16rv2" },
                { AlgorithmType.KAWPOW, "kawpow" }
            }
        };
    }
}
