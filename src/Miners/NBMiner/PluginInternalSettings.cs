using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace NBMiner
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 1, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout = DefaultTimeout,
        };

        internal static MinerBenchmarkTimeSettings BenchmarkTimeSettings = new MinerBenchmarkTimeSettings
        {
            PerAlgorithm = new Dictionary<BenchmarkPerformanceType, Dictionary<string, int>>(){
                { BenchmarkPerformanceType.Quick, new Dictionary<string, int>(){ { "KAWPOW", 160 } } },
                { BenchmarkPerformanceType.Standard, new Dictionary<string, int>(){ { "KAWPOW", 180 } } },
                { BenchmarkPerformanceType.Precise, new Dictionary<string, int>(){ { "KAWPOW", 260 } } }
            }
        };

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption> { 
                /// <summary>
                /// Comma-separated list of intensities (1-100).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nbminer_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    Delimiter = ","
                },
                /// <summary>
                /// Set intensity of cuckoo, cuckaroo, cuckatoo, [1, 12]. Smaller value means higher CPU usage to gain more hashrate. Set to 0 means autumatically adapt. Default: 0.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_intensity",
                    LongName = "--cuckoo-intensity",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Set this option to reduce the range of power consumed by rig when minining with algo cuckatoo.
                /// This feature can reduce the chance of power supply shutdown caused by overpowered.
                /// Warning: Setting this option may cause drop on minining performance.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_powerOptimize",
                    LongName = "--cuckatoo-power-optimize",
                },
                /// <summary>
                /// Generate log file named `log_<timestamp>.txt`.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_log",
                    LongName = "--log"
                },
                /// <summary>
                /// Generate custom log file. Note: This option will override `--log`.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_logFile",
                    LongName = "--log-file"
                },
                /// <summary>
                /// Use 'yyyy-MM-dd HH:mm:ss,zzz' for log time format.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_longTimeFormat",
                    LongName = "--long-format"
                },
                /// <summary>
                /// Do not query cuda device health status.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_noNvml",
                    LongName = "--no-nvml"
                },
                /// <summary>
                /// Set timeframe for the calculation of fidelity, unit in hour. Default: 24.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_fidelityTimeframe",
                    LongName = "--fidelity-timeframe",
                    DefaultValue = "24"
                },
                /// <summary>
                /// Enable memory tweaking to boost performance. comma-seperated list, range [1,6].
                /// </summary>             
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nbminer_memory-tweak",
                    ShortName = "--mt",
                    LongName = "--memory-tweak",
                    Delimiter = ","
                },
                /// <summary>
                ///  Windows only option, install / uninstall driver for memory tweak. Run with admin priviledge.
                ///  install: nbminer.exe --driver install, uninstall: nbminer.exe --driver uninstall.
                /// </summary>             
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_driver",
                    LongName = "--driver"
                },
                /// <summary>
                /// Print communication data between miner and pool in log file.
                /// </summary>   
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_verbose",
                    LongName = "--verbose",
                },
                /// <summary>
                /// set this option will disable miner interrupting current GPU jobs when a new job coming from pool,
                /// will cause less power supply issue, but might lead to a bit higher stale ratio and reject shares.
                /// </summary>  
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_noInterupt",
                    LongName = "--no-interrupt",
                },
                /// <summary>
                /// feature: add option --enable-dag-cache to allow an extra DAG for different epoch cached in GPU memory, useful for ETH+ZIL mining and mining on NiceHash.
                /// </summary>  
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_--enable-dag-cache",
                    LongName = "--enable-dag-cache",
                },
                /// <summary>
                /// feature: ethash New LHR mode for ETH mining on RTX 30 series LHR GPUs, supports Windows & Linux, able to get ~70% of maximum unlocked hashrate.
                /// This mode can be tuned by argument -lhr, only works for ethash right now.
                /// -lhr default to 0, meaning even if -lhr is not set, LHR mode with -lhr 68 will be applied to LHR GPUs if certain GPUs are detected.
                /// Tune LHR mode by setting -lhr <value>, a specific value will tell miner try to reach value percent of maximum unlocker hashrate, e.g. -lhr 68 will expect to get 68% of hashrate for same model non-LHR GPU.
                /// Higher -lhr value will results in higher hashrate, but has higher possibility to run into lock state, which will leads to much less hashrate.
                /// A good start tuning value is 68, which has been tested to be stable on most rig configurations.
                /// -lhr value can be set for each GPU by using comma separeted list, -lhr 65,68,0,-1, where -1 means turn off LHR mode.
                /// Known issue
                /// unable to unlock LHR hashrate under windows driver 471.11
                /// </summary>  
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nbminer_lhr",
                    LongName = "--lhr",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// ethash new low power LHR mode, add -lhr-mode option.
                /// -lhr-mode 2 is the default LHR mode, which is the new lower power mode.
                /// -lhr-mode 1 changes LHR mode to old version, which is the same as v39.2
                /// -lhr-mode 1 is suitable for only power limit bounded GPU, can achieve higher hashrate than mode 2
                /// -lhr-mode 2 is able to achieve lower average power and temperature. espacially suitable for GPUs with gddr6x e.g.3070ti, 3080, 3080ti.
                /// Power consumtion is fluctuating in this mode, better be used with locked core clock.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_lhrMode",
                    LongName = "--lhr-mode",
                    DefaultValue = "2"
                },
                /// <summary>
                /// -lhr-reduce-value: the amount to reduce -lhr value on a single -lhr tuning. defaults to 0.5.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lhr_reduceValue",
                    LongName = "--lhr-reduce-value",
                    DefaultValue = "0.5",
                },
                /// <summary>
                /// -lhr-reduce-time: When LHR lock is detected, and the time since the last lock exceeds this value, the -lhr reduce will not perform. defaults to 15, which means 15 minutes.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lhr_reduceTime",
                    LongName = "--lhr-reduce-time",
                    DefaultValue = "15",
                },
                /// <summary>
                /// -lhr-reduce-limit: the maximum number of times to reduce -lhr value, defaults to 6
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lhr_reduceLimit",
                    LongName = "--lhr-reduce-limit",
                    DefaultValue = "6",
                },
                /// <summary>
                /// feature: disable SNI extension for ssl connections by default, can be enabled with -enable-sni option
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "enable_sni",
                    LongName = "--enable-sni"
                },
                /// <summary>
                /// feature: add -cmd-output option to specify command line outpu to stdout or stderr, 1=stdout, 2=stderr, defaults to 2.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "cmdOutput",
                    LongName = "--cmd-output",
                    DefaultValue = "2"
                },
                /// <summary>
                /// Set power limitation of GPU.
                /// Set PL in watts: -pl 200
                /// Set PL in percentage of default PowerLimit: -pl 75% (in Windows bat file, need dual % , -pl 75%%)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "powerLimit",
                    LongName = "-power-limit",
                    ShortName = "-pl",
                    Delimiter = ","
                },
                /// <summary>
                /// Set core clock in MHz.
                /// Set clock offsets: -cclock 100 (Windows only)
                /// Set locked clocks: -cclock @1500
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "coreClock",
                    LongName = "-cclock",
                    Delimiter = ","
                },
                /// <summary>
                /// Set memory clock offsets in MHz (Windows only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "memoryClock",
                    LongName = "-mclock",
                    Delimiter = ","
                },
                /// <summary>
                /// Set locked core voltage of GPU in mV, support Turing and newer GPUs. (Windows only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lockedVoltage",
                    LongName = "-lock-cv",
                    Delimiter = ","
                },
                /// <summary>
                /// Set fan speed in percentage of GPU. (Windows only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "fanSpeed",
                    LongName = "-fan",
                    Delimiter = ","
                },
                /// <summary>
                /// Turn off the New job line in console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "logNoJob",
                    LongName = "-log-no-job",
                    Delimiter = ","
                },
                /// <summary>
                /// Set to change the cycle of Summary table show in console and log, in seconds, defaults to 30.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "logCycle",
                    LongName = "-log-cycle",
                    Delimiter = ","
                },
                /// <summary>
                /// Set power limitation of GPU.
                /// Set PL in watts: -pl 200
                /// Set PL in percentage of default PowerLimit: -pl 75% (in Windows bat file, need dual % , -pl 75%%)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "powerLimitLong",
                    LongName = "--power-limit",
                    ShortName = "--pl",
                    Delimiter = ","
                },
                /// <summary>
                /// Set core clock in MHz.
                /// Set clock offsets: -cclock 100 (Windows only)
                /// Set locked clocks: -cclock @1500
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "coreClockLong",
                    LongName = "--cclock",
                    Delimiter = ","
                },
                /// <summary>
                /// Set memory clock offsets in MHz (Windows only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "memoryClockLong",
                    LongName = "--mclock",
                    Delimiter = ","
                },
                /// <summary>
                /// Set locked core voltage of GPU in mV, support Turing and newer GPUs. (Windows only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lockedVoltageLong",
                    LongName = "--lock-cv",
                    Delimiter = ","
                },
                /// <summary>
                /// Set fan speed in percentage of GPU. (Windows only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "fanSpeedLong",
                    LongName = "--fan",
                    Delimiter = ","
                },
                /// <summary>
                /// Turn off the New job line in console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "logNoJobLong",
                    LongName = "--log-no-job",
                    Delimiter = ","
                },
                /// <summary>
                /// Set to change the cycle of Summary table show in console and log, in seconds, defaults to 30.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "logCycleLong",
                    LongName = "--log-cycle",
                    Delimiter = ","
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Set temperature limit of GPU, if exceeds, stop GPU for 10 seconds and continue.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_tempLimit",
                    LongName = "--temperature-limit"
                }
            }
        };

    }
}
