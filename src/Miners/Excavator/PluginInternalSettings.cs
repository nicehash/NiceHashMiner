using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace Excavator
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 30, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new MinerApiMaxTimeoutSetting { GeneralTimeout = DefaultTimeout };

        internal static MinerBenchmarkTimeSettings BenchmarkTimeSettings = new MinerBenchmarkTimeSettings
        {
            General = new Dictionary<BenchmarkPerformanceType, int> {
                { BenchmarkPerformanceType.Quick,    20  },
                { BenchmarkPerformanceType.Standard, 40  },
                { BenchmarkPerformanceType.Precise,  60  },
            },
        };

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// -h              Print this help and quit
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "excavator_help",
                    ShortName = "-h",
                },
                /// <summary>
                /// -p [port]       Local API port (default: 3456)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_port",
                    ShortName = "-p",
                },
                /// <summary>
                /// -i [ip]         Local API IP (default: 127.0.0.1)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_ip",
                    ShortName = "-i",
                },
                /// <summary>
                /// -wp [port]      Local HTTP API port (default: 0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_local_http_port",
                    ShortName = "-wp",
                },
                /// <summary>
                /// -wi [ip]                Local HTTP API IP (default: 127.0.0.1)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_local_http_ip",
                    ShortName = "-wi",
                },
                /// <summary>
                /// -wl [location]          Path to index.html (default: web\ (windows), web/ (linux))
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_index_location",
                    ShortName = "-wl",
                },
                /// <summary>
                /// -d [level]      Console print level (0 = print all, 5 = fatal only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_console_print_level",
                    ShortName = "-d",
                },
                /// <summary>
                /// -f [level]      File print level (0 = print all, 5 = fatal only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_file_print_level",
                    ShortName = "-f",
                },
                /// <summary>
                /// -fn [file]      Log file (default: log_$timestamp.log)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_log_filename",
                    ShortName = "-fn",
                },
                /// <summary>
                /// -c [file]       Use command file (default: none)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_command_file",
                    ShortName = "-c",
                },
                /// <summary>
                /// -t [level]      Use test(dev) network (default: 0 = production, 1 = test, 2 = testdev)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_mining_network",
                    ShortName = "-t",
                },
                /// <summary>
                /// -m              Allow multiple instances of Excavator
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "excavator_allow_multiple_instances",
                    ShortName = "-m",
                },
                /// <summary>
                /// -ql [location]  QuickMiner location ('eu' or 'usa')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_quickminer_location",
                    ShortName = "-ql",
                },
                /// <summary>
                /// -qu [username]  QuickMiner username
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "excavator_quickminer_username",
                    ShortName = "-qu",
                },
                /// <summary>
                /// -qc             QuickMiner cpu mining active
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "excavator_quickminer_cpu_mining_active",
                    ShortName = "-qc",
                },
                /// <summary>
                /// -qm             QuickMiner minimize
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "excavator_quickminer_minimize",
                    ShortName = "-qm",
                },
                /// <summary>
                /// -qx             No QuickMiner
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "excavator_no_quickminer",
                    ShortName = "-qx",
                },
            },
            TemperatureOptions = new List<MinerOption> { }
        };

    }
}
