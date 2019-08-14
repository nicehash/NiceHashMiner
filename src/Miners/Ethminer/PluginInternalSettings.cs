using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace Ethminer
{
    internal static class PluginInternalSettings
    {
        // TODO update these
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// WHAT??
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "Ethminer_XX",
                    ShortName = "-XX",
                    DefaultValue = ""
                },
            },
            TemperatureOptions = new List<MinerOption>{}
        };
    }
}
