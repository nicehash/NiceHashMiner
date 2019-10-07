using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace XMRig
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        { 
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// number of miner threads
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_threads",
                    ShortName = "-t",
                    LongName = "--threads="
                },
                /// <summary>
                /// set process affinity to CPU core(s), mask 0x3 for cores 0 and 1
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_cpu_affinity",
                    LongName = "--cpu-affinity"
                },
                /// <summary>
                /// set process priority (0 idle, 2 normal to 5 highest)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_cpu_priority",
                    LongName = "--cpu-priority"
                },
                /// <summary>
                /// disable huge pages support
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "xmrig_no_huge_pages",
                    LongName = "--no-huge-pages"
                },
                /// <summary>
                /// disable colored output
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "xmrig_no_color",
                    LongName = "--no-color"
                },
                /// <summary>
                /// donate level, default 5% (5 minutes in 100 minutes)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_donate_level",
                    LongName = "--donate-level="
                },
                /// <summary>
                /// set custom user-agent string for pool
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_user_agent",
                    LongName = "--user-agent"
                },
                /// <summary>
                /// run the miner in the background
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "xmrig_background",
                    ShortName = "-B",
                    LongName = "--background"
                },
                /// <summary>
                /// load a JSON-format configuration file
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_config",
                    ShortName = "-c",
                    LongName = "--config="
                },
                /// <summary>
                /// log all output to a file
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_log",
                    ShortName = "-l",
                    LongName = "--log-file="
                },
                /// <summary>
                /// ASM optimizations, possible values: auto, none, intel, ryzen, bulldozer
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_asm",
                    LongName = "--asm="
                },
                /// <summary>
                /// print hashrate report every N seconds
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_print_time",
                    LongName = "--print-time="
                }
            }
        };

    }
}
