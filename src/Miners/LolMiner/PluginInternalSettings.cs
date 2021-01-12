using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace LolMiner
{
    internal static class PluginInternalSettings
    {
        internal static MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
                {"GPU_FORCE_64BIT_PTR", "1"},
                {"GPU_USE_SYNC_OBJECTS", "1"}
            }
        };

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Enables printing a log file; --log [=arg(=on)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableLogs",
                    ShortName = "--log"
                },
                /// <summary>
                /// Path to a custom log file location
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_logFile",
                    ShortName = "--logfile"
                },

                /// <summary>
                /// Long statistics interval
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_longStats",
                    ShortName = "--longstats",
                    DefaultValue = "150"
                },
                /// <summary>
                /// Short statistics interval
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_shortStats",
                    ShortName = "--shortstats",
                    DefaultValue = "30"
                },
                /// <summary>
                /// Number of digits in hash speed after delimiter
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_decimalDigits",
                    ShortName = "--digits"
                },
                /// <summary>
                /// Enables time stamp on short statistics ("on" / "off"); --timeprint [=arg(=on)] (=off)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableTimestamps",
                    ShortName = "--timeprint"
                },
                /// <summary>
                /// Sets the memory size (in MByte) the
                /// miner is allowed for Ethash on 4G
                /// cards. Suggested values: Windows: 4024
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lolMiner_4GAllocSize",
                    ShortName = "--4g-alloc-size",
                },
                /// <summary>
                /// Windows: added experimental mem allocation pattern that should allow reaching epoch 375 or 376 at full speed ( * ).
                /// It is default on in Windows, you can turn it off with "--win4galloc 0"
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_mem4GAlloc",
                    ShortName = "--win4galloc",
                },
                /// <summary>
                /// Set the number of MBytes of GPU memory that should be left free by the miner.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_keepFree",
                    ShortName = "--keepfree",
                },
                /// <summary>
                /// This will disable the 2nd mining thread and slightly reduce performance of the involved cards.
                /// Use this option to reduce stumbles when a card does graphic output in parallel.
                /// Use --singlethread to set the mode for one single card
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "lolMiner_SingleThread",
                    ShortName = "--singlethread",
                },
                /// <summary>
                /// for Polaris GPUs. This will increase the performance of zombie mode (further up on the general improvement) by an other 5-15%, depending on parameter and epoch (later epochs profit more).
                /// Default value is 0 (off), for most cards the value of 2 is optimal. If you see cards getting slower then before, set to 0 or 1.
                /// Note: you either can give one value for the whole rig or provide a comma separated list for each card individually. Cards not running zombie mode ignore the parameter.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lolMiner_zombieTune",
                    ShortName = "--zombie-tune",
                    Delimiter = ",",
                    DefaultValue = "0"
                }
            }
        };
    }
}
