using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace CryptoDredge
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Mining intensity (0 - 6). (default: 6)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "cryptodredge_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    DefaultValue = "6",
                    Delimiter = ","
                },
                /// <summary>
                /// Set process priority in the range 0 (low) to 5 (high). (default: 3)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID  = "cryptodredge_cpu_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "3"
                },
                /// <summary>
                /// Log output to file
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID  = "cryptodredge_log",
                    ShortName = "--log"
                },
                /// <summary>
                /// JSON configuration file to use (default: config.json)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID  = "cryptodredge_config",
                    ShortName = "-c",
                    LongName = "--config"
                }
            }
        };
    }
}
