using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace Phoenix
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption> {
                /// <summary>
                /// Turn on AMD compute mode on the supported GPUs. This is equivalent of pressing 'y' in the miner console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "phoenix_acm",
                    ShortName = "-acm"
                },
                /// <summary>
                /// Set the mining intensity (0 to 14; 12 is the default for new kernels). You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_mi",
                    ShortName = "-mi",
                    DefaultValue = "12",
                    Delimiter = ","
                },
                /// <summary>
                /// Set the GPU tuning parameter (6 to 400). The default is 15. You can change the tuning parameter interactively with the '+' and '-' keys in the miner's console window.
                /// You may specify this option per-GPU. If you don't specify -gt or you specify value 0, the miner will use auto-tuning to determine the best GT value.
                /// Note that when the GPU is dual-mining, it ignores the -gt values, and uses -sci instead.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_gt",
                    ShortName = "-gt",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set the dual mining intensity (1 to 1000). The default is 30. As you increase the value of -sci, the secondary coin hashrate will increase but the price will be higher power consumption and/or lower ethash hashrate.
                /// You can change the this parameter interactively with the '+' and '-' keys in the miner's console window. You may specify this option per-GPU.
                /// If you set -sci to 0, the miner will use auto-tuning to determine the best value, while trying to maximize the ethash hashrate regardless of the secondary coin hashrate.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_sci",
                    ShortName = "-sci",
                    DefaultValue = "30",
                    Delimiter = ","
                },
                /// <summary>
                /// Type of OpenCL kernel: 0 - generic, 1 - optimized, 2 - alternative, 3 - turbo (1 is the default). You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_clKernel",
                    ShortName = "-clKernel",
                    DefaultValue = "1",
                    Delimiter = ","
                },
                /// <summary>
                /// Use the power-efficient ("green") kernels (0: no, 1: yes; default: 0).
                /// You may specify this option per-GPU. Note that you have to run auto-tune again as the optimal GT values are completely different for the green kernels
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_clgreen",
                    ShortName = "-clgreen",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Use new AMD kernels if supported (0: no, 1: yes; default: 1). You may specify this option per-GPU.
                /// </summary>

                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_clNew",
                    ShortName = "-clNew",
                    DefaultValue = "1",
                    Delimiter = ","
                },
                /// <summary>
                /// AMD kernel sync (0: never, 1: periodic; 2: always; default: 1). You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_clf",
                    ShortName = "-clf",
                    DefaultValue = "1",
                    Delimiter = ","
                },
                /// <summary>
                /// Type of Nvidia kernel: 0 auto (default), 1 old (v1), 2 newer (v2), 3 latest (v3).
                /// Note that v3 kernels are only supported on GTX10x0 GPUs. 
                /// Also note that dual mining is supported only by v2 kernels. You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_nvKernel",
                    ShortName = "-nvKernel",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Enable Nvidia driver-specific optimizations (0 - no, the default; 1 - yes). Try -nvdo 1 if your are unstable. You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_nvdo",
                    ShortName = "-nvdo",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Use new Nvidia kernels if supported (0: no, 1: yes; default: 1). You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_nvNew",
                    ShortName = "-nvNew",
                    DefaultValue = "1",
                    Delimiter = ","
                },
                /// <summary>
                /// Nvidia kernel sync (0: never, 1: periodic; 2: always; 3: forced; default: 1). You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_nvf",
                    ShortName = "-nvf",
                    DefaultValue = "1",
                    Delimiter = ","
                },
                /// <summary>
                /// Restart the miner if avg 5 min speed is below <n> MH/s
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_mingRigSpeed",
                    ShortName = "-minRigSpeed"
                },
                /// <summary>
                /// Allocate DAG buffers big enough for n epochs ahead (default: 2) to avoid allocating new buffers on each DAG epoch switch, which should improve DAG switch stability.
                /// You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_eres",
                    ShortName = "-eres",
                    DefaultValue = "2",
                    Delimiter = ","
                },
                /// <summary>
                /// Slow down DAG generation to avoid crashes when switching DAG epochs (0-3, default: 0 - fastest, 3 - slowest). You may specify this option per-GPU.
                /// Currently the option works only on AMD cards
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_lidag",
                    ShortName = "-lidag",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Serializing DAG creation on multiple GPUs (0 - no serializing, all GPUs generate the DAG simultaneously, this is the default;
                /// 1 - partial overlap of DAG generation on each GPU; 2 - no overalp(each GPU waits until the previous one has finished generating the DAG);
                /// 3-10 - from 1 to 8 seconds delay after each GPU DAG generation before the next one)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_gser",
                    ShortName = "-gser",
                    DefaultValue = "0",
                },
                /// <summary>
                /// Use alternative way to initialize AMD cards to prevent startup crashes
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "phoenix_altinit",
                    ShortName = "-altinit",
                },
                /// <summary>
                /// Selects the log file mode:
                /// 0: disabled - no log file will be written
                /// 1: write log file but don't show debug messages on screen (default)
                /// 2: write log file and show debug messages on screen
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_log",
                    ShortName = "-log",
                    DefaultValue = "1"
                },
                /// <summary>
                /// Set the name of the logfile. If you place an asterisk (*) in the logfile name, it will be
                /// replaced by the current date/time to create a unique name every time PhoenixMiner is started.
                /// If there is no asterisk in the logfile name, the new log entries will be added to end of the same file.
                /// If you want to use the same logfile but the contents to be overwritten every time when you start the miner,
                /// put a dollar sign ($) character in the logfile name (e.g. -logfile my_log.txt$).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_logfile",
                    ShortName = "-logfile",
                },
                /// <summary>
                /// Set a path where the logfile(s) will be created
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_logDir",
                    ShortName = "-logdir",
                },
                /// <summary>
                /// Maximum size of the logfiles in MB. The default is 200 MB (use 0 to turn off the limitation).
                /// On startup, if the logfiles are larger than the specified limit, the oldest are deleted.
                /// If you use a single logfile (by using -logfile), then it is truncated if it is bigger than the limit and a new one is created.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_logsmaxsize",
                    ShortName = "-logsmaxsize",
                },
                /// <summary>
                /// Lower the GPU usage to n% of maximum (default: 100). If you already use -mi 0 (or other low value) use -li instead.
                /// You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_gpow",
                    ShortName = "-gpow",
                    DefaultValue = "100",
                    Delimiter = ","
                },
                /// <summary>
                /// Another way to lower the GPU usage. Bigger n values mean less GPU utilization; the default is 0.
                /// You may specify this option per-GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_li",
                    ShortName = "-li",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Reset the HW overclocking settings on startup
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "phoenix_resetOC",
                    ShortName = "-resetoc"
                },
                /// <summary>
                /// Do not reset overclocking settings when closing the miner
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "phoenix_leaveOC",
                    ShortName = "-leaveoc"
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Set fan control target temperature
                /// (special values: 0 - no HW monitoring on ALL cards, 1-4 - only monitoring on all cards with 30-120 seconds interval, negative - fixed fan speed at n %)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_tt",
                    ShortName = "-tt",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set fan control min speed in % (-1 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_fanmin",
                    ShortName = "-fanmin",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// Set fan control max speed in % (-1 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_fanmax",
                    ShortName = "-fanmax",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// Set fan control mode (0 - auto, 1 - use VBIOS fan control, 2 - forced fan control; default: 0)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_fcm",
                    ShortName = "-fcm",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set fan control max temperature (0 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_tmax",
                    ShortName = "-tmax",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set GPU power limit in % (from -75 to 75, 0 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_powlim",
                    ShortName = "-powlim",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set GPU core clock in MHz (0 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_cclock",
                    ShortName = "-cclock",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set GPU core voltage in mV (0 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_cvddc",
                    ShortName = "-cvddc",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set GPU memory clock in MHz (0 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_mclock",
                    ShortName = "-mclock",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Set GPU memory voltage in mV (0 for default)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_mvddc",
                    ShortName = "-mvddc",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Pause a GPU when temp is >= n deg C (0 for default; i.e. off)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_tstop",
                    ShortName = "-tstop",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Resume a GPU when temp is <= n deg C (0 for default; i.e. off)
                /// You may specify this option per-GPU. Only AMD cards.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "phoenix_tstart",
                    ShortName = "-tstart",
                    DefaultValue = "0",
                    Delimiter = ","
                }
            }
        };

        internal static MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            // we have same env vars for all miners now, check avemore env vars if they differ and use custom env vars instead of defaults
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_USE_SYNC_OBJECTS", "1"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
                {"GPU_FORCE_64BIT_PTR", "0"}
            },
        };
    }
}
