using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
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
                    ShortName = "-clGreen",
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
                /// Allow a few more weeks of work for 4 GB AMD Polaris cards. DAG size limit (0 - off, 1 - auto, >1000 - DAG size limit in MB)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_daglimit",
                    ShortName = "-daglim"
                },          
                /// <summary>
                /// Restart the miner when allocating buffer for a new DAG epoch.
                /// The possible values are: 0 - never, 1 - always, 2 - auto (the miner decides depending on the driver version).
                /// This is relevant for 4 GB AMD cards, which may have problems with new DAG epochs after epoch 350.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_dagrestart",
                    ShortName = "-dagrestart"
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
                /// -rvram <n> Minimum free VRAM in MB (-1: don't check; default: 384 for Windows, and 128 for Linux)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_rvram",
                    ShortName = "-rvram"
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
                /// Load a file with configuration options that will be added to the command-line options.
                /// Note that the order is important. For example, if we have a config.txt file that contains -cclock 1000
                /// and we specify command line -cclock 1100 -config config.txt, the options from the config.txt file will take
                /// precedence and the resulting -cclock will be 1000. If the order is reversed (-config config.txt -cclock 1100)
                /// then the second option takes precedence and the resulting -cclock will be 1100. Note that only one -config
                /// option is allowed. Also note that if you reload the config file with 'c' key or with the remote interface,
                /// its options will take precedence over whatever you have specified in the command-line.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_config",
                    ShortName = "-config",
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
                },
                /// <summary>
                /// Memory strap level (Nvidia cards 10x0 series only). The possible
                /// values are 0 to 6. 0 is the default value and uses the 
                /// timings from the VBIOS. Each strap level corresponds to a 
                /// predefined combination of memory timings ("-vmt1", "-vmt2", "-vmt3", "-vmr").
                /// Strap level 3 is the fastest predefined level and may not work on most cards, 1 is the slowest (but still faster than 
                /// the default timings). Strap levels 4 to 6 are the same as 1 to 3 
                /// but with less aggressive refresh rates (i.e. lower "-vmr" values).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_straps",
                    ShortName = "-straps"
                },
                /// <summary>
                /// Memory timing parameter 1 (0 to 100, default 0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_vmt1",
                    ShortName = "-vmt1"
                },
                /// <summary>
                /// Memory timing parameter 2 (0 to 100, default 0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_vmt2",
                    ShortName = "-vmt2"
                },
                /// <summary>
                /// Memory timing parameter 3 (0 to 100, default 0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_vmt3",
                    ShortName = "-vmt3"
                },
                /// <summary>
                /// Memory refresh rate (0 to 100, default 0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_vmr",
                    ShortName = "-vmr"
                },
                /// <summary>
                /// Force using straps on unsupported Nvidia GPUs (0 - do not force, 1 - GDDR5, 2 - GDDR5X).
                /// Make sure that the parameter matches your GPU memory type.
                /// You can try this if your card is Pascal-based but when you try to use -straps or any other memory timing option,
                /// the card is shown as “unsupported”.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_nvmem",
                    ShortName = "-nvmem"
                },
                /// <summary>
                /// Memory refresh rate on AMD cards (0 - default values, 1 - predefined value that should work on most cards, 2 to 100 -
                /// increasingly aggressive settings). If you want to fine tune the value, you may run the miner with "-rxboost 1", write down the
                /// corresponding "-vmr" values that are showed in the log file, and then use "-vmr" instead with adjusted values.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_rxboost",
                    ShortName = "-rxboost"
                },
                /// <summary>
                /// Reset the memory overclock on Nvidia cards during DAG generation.
                /// This may allow you to set higher memory overclock on your Nvidia cards without risking corrupt DAG buffer, which can lead to excessive number of incorrect shares.
                /// Use -mcdag 1 (by default the value is 0, which means turned off) to use this new feature.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_mcdag",
                    ShortName = "-mcdag",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Submit stales to ethash pool: 1 - yes, 0 - no (default)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_stales",
                    ShortName = "-stales",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Price of the electricity in USD per kWh (e.g. -prate 0.1).
                /// If specified the miner will calculate the rig daily electricity cost
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_prate",
                    ShortName = "-prate"
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Lower GPU usage when GPU temperature is above n deg C. The default value is 0,
                /// which means do not lower the usage regardless of the GPU temperature. This option is useful whenever -tmax is not
                /// working. If you are using both "-tt" and "-ttli" options, the
                /// temperature in "-tt" should be lower than the "-ttli" to avoid throttling the GPUs without using the fans
                /// to properly cool them first.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_ttli",
                    ShortName = "-ttli"
                },
                /// <summary>
                /// Frequency of hardware monitoring (one setting for all cards, the
                /// default is 1): 0 - no HW monitoring or control, 1 - normal
                /// monitoring, 2 to 5 - less frequent monitoring.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_hwm",
                    ShortName = "-hwm",
                    DefaultValue = "1"
                },
                /// <summary>
                /// Set fan control target temperature (special values: 0 - no fan control, negative - fixed fan speed at n %)
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
                /// Memory timing level (0 - VBIOS/default)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_mt",
                    ShortName = "-mt"
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
                },
                /// <summary>
                /// <n> Level of hardware monitoring: 0 - temperature and fan speed only; 1 - temperature, fan speed, and power;
                /// 2 - full (include core/memory clocks, voltages, P-states). The default is 1.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "phoenix_hstats",
                    ShortName = "-hstats"
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
