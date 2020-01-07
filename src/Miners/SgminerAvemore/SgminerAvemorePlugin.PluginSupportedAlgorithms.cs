using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace SgminerAvemore
{
    public partial class SgminerAvemorePlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.X16R)
                        {
                            ExtraLaunchParameters = "-X 256"
                        },
                    }
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.X16R, "x16r" },
            }
        };
    }
}
