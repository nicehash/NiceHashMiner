using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
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
                /// maximum CPU threads count (in percentage) hint for autoconfig
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_cpu_max_threads",
                    LongName = "--cpu-max-threads-hint="
                },
                /// <summary>
                /// number of 2 MB pages for persistent memory pool, -1 (auto), 0 (disable)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_cpu_mem_pool",
                    LongName = "--cpu-memory-pool="
                },
                /// <summary>
                /// prefer maximum hashrate rather than system response/stability
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "xmrig_cpu_no_yield",
                    LongName = "--cpu-no-yield"
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
                /// ASM optimizations, possible values: auto, none, intel, ryzen, bulldozer
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_asm",
                    LongName = "--asm="
                },
                /// <summary>
                /// threads count to initialize RandomX dataset
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_rx_init",
                    LongName = "--randomx-init="
                },
                /// <summary>
                /// disable NUMA support for RandomX
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "xmrig_rx_no_numa",
                    LongName = "--randomx-no-numa"
                },
                /// <summary>
                /// RandomX mode: auto, fast, light
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_rx_mode",
                    LongName = "--randomx-mode="
                },
                /// <summary>
                /// write custom value (0-15) to Intel MSR register 0x1a4 or disable MSR mod (-1)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_wrmsr",
                    LongName = "--randomx-wrmsr="
                },
                /// <summary>
                /// disable reverting initial MSR values on exit
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "xmrig_no_rdmsr",
                    LongName = "--randomx-no-rdmsr"
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
                /// print hashrate report every N seconds
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_print_time",
                    LongName = "--print-time="
                },
                /// <summary>
                /// print health report every N seconds
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "xmrig_health_print_time",
                    LongName = "--health-print-time="
                },
                /// <summary>
                /// disable colored output
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "xmrig_no_color",
                    LongName = "--no-color"
                }
            }
        };

    }
}
