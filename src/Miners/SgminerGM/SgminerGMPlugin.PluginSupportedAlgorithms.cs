using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace SgminerGM
{
    public partial class SgminerGMPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto)
                        {
                            ExtraLaunchParameters = " --remove-disabled --xintensity 512 -w 192 -g 1"
                        },
                    }
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.DaggerHashimoto, "ethash" },
            }
        };
    }
}
