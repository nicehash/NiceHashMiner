using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
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
                /// If no filename is provided, the miner will log to trm_<algo>_<yyyymmdd_hhmmss>.log in the current working directory.
                /// If the log file already exists, the miner will append.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_logFile",
                    ShortName = "--log_file="
                },
                /// <summary>
                /// (Windows only) Enables compute mode and disables crossfire on necessary gpus.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_enableCompute",
                    ShortName = "--enable_compute"
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
                /// <summary>
                /// Tests a specific ethash epoch.
                /// NOTE: you still need to provide a pool as if you were mining, but no shares will be submitted.
                /// Simulated mining only.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_ethEpoch",
                    ShortName = "--eth_epoch",
                },
                /// <summary>
                ///  Enables automatic use of the 'B' mode for all Polaris 8GB cards, unless they have a 
                ///  different config provided by the --eth_config argument.  This is the same thing as 
                ///  manually setting all Polaris 8GB gpus in the rig to 'B' mode using --eth_config. 
                ///  For most gpus, this adds 0.1-0.2 MH/s of hashrate.NOTE: 20-25% of rigs becomes less 
                ///  stable in this mode which is the reason it isn't the default mode.  If you experience 
                ///  dead gpus, you should remove this argument and run the gpus in the 'A' mode.Moreover, 
                ///  this option will stop working when the DAG approaches 4GB.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_ethAggrMode",
                    ShortName = "--eth_aggr_mode",
                },
                /// <summary>
                ///  anual ethash configuration for the miner.  CONFIG must be in the form [M][L].
                ///  The [M] value selects the mode which can be 'A','B', or 'C'.
                ///  The 'B' mode uses additional memory and will only work on 8+GB cards.
                ///  The 'C' mode uses additional memory and will only work on 16+GB cards, such as the VII, with
                ///  a correctly configured system.  See the ETHASH_TUNING_GUIDE.txt for more details.
                ///  The [L] value selects the intensity and it's range will depend on the GPU architecture.
                ///  Both values are optional, but if [L] is specified, [M] must also be specified.
                ///  Example configs: --eth_config = A
                ///  --eth_config = B750
                ///  CONFIG can also be a comma separated list of config values where each is
                ///  applied to each GPU.For example: --eth_config = A, B750,, A288
                ///  Any gpu that does not have a specific config in the list will use the first
                ///  config in the list.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "teamRedMiner_ethConfig",
                    ShortName = "--eth_config=",
                },
                /// <summary>
                ///  Configures the gpu watchdog to shut down the miner and run the specified platform
                ///  and exits immediately. The default script is watchdog.bat/watchdog.sh in the
                ///  current directory, but a different script can be provided as an optional argument,
                ///  potentially with a absolute or relative path as well.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_watchdogScriptDefault",
                    ShortName = "--watchdog_script",
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "teamRedMiner_watchdogScript",
                    ShortName = "--watchdog_script=",
                },
                /// <summary>
                ///  Tests the configured watchdog script by triggering the same action as a dead gpu
                ///  after ~20 secs of mining.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_watchdogTest",
                    ShortName = "--watchdog_test",
                },
                /// <summary>
                ///  Forces the watchdog to not execute. Can be used to disable the watchdog in mining OS
                ///  that always run with the watchdog enabled.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_watchdogDisabled",
                    ShortName = "--watchdog_disabled",
                },
                /// <summary>
                ///  On Windows, the allocation balance is very delicate for 4GB gpus being able to reach their
                ///  maximum possible DAG epoch.  The miner uses a strategy that has worked fine for our test gpus,
                ///  but other setups can benefit from tweaking this number.  The valid range is [-128,+128].  Zero means
                ///  no adjustment.  You provide either a single value that is used for all 4GB gpus in the rig, or a
                ///  comma-separated list with values for all gpus, including non-4GB Polaris gpus.
                ///  Values for non-4GB gpus are ignored.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "teamRedMiner_4gAllocAdjust",
                    ShortName = "--eth_4g_alloc_adjust=",
                },
                /// <summary>
                ///  This argument allows mining on 4GB gpus after they no longer can store the full DAG in vram.
                ///  You pass either the max epoch to allocate memory for, or the raw nr of MB to allocate.  You can
                ///  provide a single value that applies to all 4GB gpus in the rig, or use a comma-separated list for
                ///  specifying different values per gpu.  Values for non-4GB gpus are ignored.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "teamRedMiner_4gMaxAlloc",
                    ShortName = "--eth_4g_max_alloc=",
                },
                /// <summary>
                ///  Enables staggering of gpus when building a new DAG.  This is more lean on PSUs that don't like
                ///  going from 0-100% load on all gpus at the same time.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_EthStagger",
                    ShortName = "--eth_stagger",
                },
                /// <summary>
                ///  Adds ramping up the intensity on all gpus after a DAG build, gpu disable/enable or network outage.
                ///  Can help rigs with crashes right between the DAG build and starting mining.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "teamRedMiner_EthRampUp",
                    ShortName = "--eth_ramp_up",
                }
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
