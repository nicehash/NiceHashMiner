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
                }
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
