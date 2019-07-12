using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace BMiner
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// The sub-solver for dual mining. Valid values are 0, 1, 2, 3. Default is -1, which is to tune automatically. (default -1)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_dual_subsolver",
                    ShortName = "-dual-subsolver",
                    DefaultValue = "-1"
                },
                /// <summary>
                /// The intensity of the CPU for grin/AE mining. Valid values are 0 to 12. Higher intensity may give better performance but more CPU usage. (default 6)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_cpu_intensity",
                    ShortName = "-intensity",
                    DefaultValue = "6"
                },
                /// <summary>
                /// Append the logs to the file <path>.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_logfile",
                    ShortName = "-logfile="
                },
                /// <summary>
                /// Disable runtime information collection for Bminer.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "bminer_noRuntimeInfo",
                    ShortName = "-no-runtime-info"
                },
                /// <summary>
                /// Remove timestamp in your logging messages.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "bminer_noTimestamps",
                    ShortName = "-no-timestamps"
                },
                /// <summary>
                /// Disable the devfee but it also disables some optimizations.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "bminer_nofee",
                    ShortName = "-nofee"
                },
                /// <summary>
                /// Personalization string for equihash 144,5 based coins. Default: BgoldPoW. Valid values include BitcoinZ, Safecoin, ZelProof, etc. (default "BgoldPoW")
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_pers",
                    ShortName = "-pers",
                    DefaultValue = "BgoldPoW"
                }
            },
            TemperatureOptions = new List<MinerOption>{
                /// <summary>
                /// Hard limits of the temperature of the GPUs. Bminer slows down itself when the temperautres of the devices exceed the limit. (default 85)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_max_temp",
                    ShortName = "-max-temperature",
                    DefaultValue = "85"
                }
            }
        };
    }
}
