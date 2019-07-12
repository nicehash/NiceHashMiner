using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace TeamRedMiner
{
    internal static class PluginInternalSettings
    {
        internal static MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_USE_SYNC_OBJECTS", "1"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
            },
        };

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Specified the init style (1 is default):
                /// 1: One gpu at the time, complete all before mining.
                /// 2: Three gpus at the time, complete all before mining.
                /// 3: All gpus in parallel, start mining immediately.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_initStyle",
                    ShortName = "--init_style=",
                    DefaultValue = "1"
                },
                /// <summary>
                /// Enables debug log output.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_debug",
                    ShortName = "--debug",
                },
                /// <summary>
                /// Set the time interval in seconds for averaging and printing GPU hashrates.
                /// SEC sets the interval in seconds, and must be > 0.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_logInterval",
                    ShortName = "-l",
                    LongName = "--log_interval="
                },
                /// <summary>
                /// Enables logging of miner output into the file specified by FILENAME.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_logFile",
                    ShortName = "--log_file="
                },
                /// <summary>
                /// Manual cryptonight configuration for the miner.  CONFIG must be in the form
                /// [P][I0][M][I1][:xyz], where [P] is an optional prefix and [:xyz] is an
                /// optional suffix.  For [P], only the value of 'L' is supported for low-end
                /// GPUs like Lexa/Baffin.  [I0] and [I1] are the thread intensity values normally
                /// ranging from 1 to 16, but larger values are possible for 16GB gpus.  [M] is the
                /// mode which can be either '.', -', '+' or '*'.  Mode '.' means that the miner
                /// should choose or scan for the best mode.  Mode '*' both a good default more and
                /// _should_ be used if you mine on a Vega 56/64 with modded mem timings.  The
                /// exceptions to this rule are small pad variants (cnv8_trtl and cnv8_upx2), they
                /// should still use '+'. For Polaris gpus, only the '-' and '+' modes are available.
                /// NOTE: in TRM 0.5.0 auto-tuning functionality was added, making manual configuration
                /// of the CN config modes unnecessary except for rare corner cases.  For more info,
                /// see the tuning docs and how-to documents bundled with the release.
                /// Example configs: --cn_config=15*15:AAA
                /// --cn_config=L4+3
                /// CONFIG can also be a comma seperated list of config values where each is
                /// applied to each GPU. For example: --cn_config=8-8,16+14:CBB,15*15,14-14
                /// Any gpu that does not have a specific config in the list will use the first
                /// config in the list.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "teamRedMiner_cnConfig",
                    ShortName = "--cn_config=",
                    Delimiter = ","
                },
                /// <summary>
                /// Disables cpu verification of found shares before they are submitted to the pool.
                /// Note: only CN algos currently supports cpu verification.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_noCpuCheck",
                    ShortName = "--no_cpu_check",
                },
                /// <summary>
                /// Disables the CN lean mode where ramp up threads slowly on start or restart after
                /// network issues or gpu temp throttling.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_noLean",
                    ShortName = "--no_lean",
                },
                /// <summary>
                /// Lists gpu devices where CN thread interleave logic should be not be used.
                /// The argument is a comma-separated list of devices like for the -d option.
                /// Use this argument if some device(s) get a worse hashrate together with a lot
                /// of interleave adjust log messages.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_noInterleave",
                    ShortName = "--no_interleave=",
                },
                /// <summary>
                /// Enable the auto-tune mode upon startup. Only available for CN variants. MODE must
                /// be either NONE, QUICK or SCAN. The QUICK mode checks a few known good configurations
                /// and completes within 1 min. The SCAN mode will check all possible combos and will
                /// run for 20-30 mins. Setting MODE to NONE disable the auto-tune feature. The default
                /// mode is QUICK.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_autoTuneMode",
                    ShortName = "--auto_tune=",
                    DefaultValue = "QUICK"
                },
                /// <summary>
                /// Executes multiple runs for the auto tune, each time decreasing the unit of pads used -1
                /// in one of the threads (15+15 -> 15+14 -> 14+14 -> 14+13 -> ...). You can specify the
                /// explicit nr of runs or let the miner choose a default value per gpu type (typically 3-4).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_autoTuneRuns",
                    ShortName = "--auto_tune_runs=",
                },
                /// <summary>
                /// If present, and when the driver indicates there is enough GPU vram available, the miner
                /// will be more aggressive with the initial memory allocation. In practice, this option
                /// means that Vega GPUs under Linux will start the auto-tuning process at 16*15 rather
                /// than 16*14 or 15*15.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_largeAlloc",
                    ShortName = "--allow_large_alloc",
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                ///  Sets the temperature at which the miner will stop GPUs that are too hot.
                ///  Default is 85C.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_tempLimit",
                    ShortName = "--temp_limit=",
                    DefaultValue = "85"
                },
                /// <summary>
                ///  Sets the temperature below which the miner will resume GPUs that were previously stopped due to temperature exceeding limit.
                ///  Default is 60C.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_tempResume",
                    ShortName = "--temp_resume=",
                    DefaultValue = "60"
                }
            }
        };
    }
}
