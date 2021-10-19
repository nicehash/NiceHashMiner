using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace TRex
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
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Allocate extra DAG at GPU for specified epoch. Can be useful for dual mining
                /// of coins like Zilliqa (ZIL). (eg: --extra-dag-epoch 0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_extraDagEpoch",
                    LongName = "--extra-dag-epoch"
                },
                /// <summary>
                /// GPU intensity 8-25 (default: auto).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    DefaultValue = "auto"
                },
                /// <summary>
                /// [Ethash] Choose CUDA kernel (default: 0). Range from 0 to 5.
                /// Set to 0 to enable auto-tuning: the miner will benchmark each kernel and select the fastest.
                /// Can be set to a comma separated list to apply different values to different cards.
                /// (eg: --kernel 2,1,1,3)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_kernel",
                    LongName = "--kernel",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                ///  Low load mode (default: 0). 1 - enabled, 0 - disabled.
                ///  Reduces the load on the GPUs if possible. Can be set to a comma separated string to enable
                ///  the mode for a subset of the GPU list (eg: --low-load 0,0,1,0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_lowLoad",
                    ShortName = "--low-load",
                },
                /// <summary>
                ///  Continue mining even in case of connection loss.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_keepGpuBusy",
                    ShortName = "--keep-gpu-busy",
                },            
                /// <summary>
                /// Set temperature color for GPUs stat.
                /// Example: 55,65 - it means that temperatures above 55 will have yellow color, above 65 - red color.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempColor",
                    LongName = "--temperature-color"
                },  
                /// <summary>
                ///  parameter to forbid applying config changes via API and web-monitoring page
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_apiReadOnly",
                    ShortName = "--api-read-only",
                },
                /// <summary>
                /// Sliding window length in seconds used to compute average hashrate (default: 60).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_avgHashrate",
                    ShortName = "-N",
                    LongName = "--hashrate-avr",
                    DefaultValue = "60"
                },
                /// <summary>
                /// Sliding window length in seconds used to compute sharerate (default: 600).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_avgSharerate",
                    ShortName = "--sharerate-avr",
                    DefaultValue = "600"
                },
                /// <summary>
                /// GPU stats report frequency. Minimum is 5 sec. (default: 30 sec)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_reportInterval",
                    ShortName = "--gpu-report-interval",
                    DefaultValue = "30"
                },
                /// <summary>
                /// Quiet mode. No GPU stats at all.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_quiet",
                    ShortName = "-q",
                    LongName = "--quiet"
                },
                /// <summary>
                /// Don't show date in console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_hideDate",
                    ShortName = "--hide-date"
                },
                /// <summary>
                /// Disable color output for console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_noColor",
                    ShortName = "--no-color"
                },
                /// <summary>
                /// Disable NVML GPU stats.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_noNvml",
                    ShortName = "--no-nvml"
                },
                /// <summary>
                ///  parameter to disable hashrate reporting to the mining pool
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_noHashrateReport",
                    ShortName = "--no-hashrate-report",
                },
                /// <summary>
                /// Full path of the log file.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_logPath",
                    ShortName = "-l",
                    LongName = "--log-path"
                },
                /// <summary>
                /// Set process priority (default: 2) 0 idle, 2 normal to 5 highest.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "2"
                },
                /// <summary>
                /// Forces miner to immediately reconnect to pool on N successively failed shares (default: 10).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_reconectFailed",
                    ShortName = "--reconnect-on-fail-shares",
                    DefaultValue = "10"
                },
                /// <summary>
                /// Memory tweak mode (default: 0 - disabled). Range from 0 to 6. General recommendation
                /// is to start with 1, and then increase only if the GPU is stable.
                /// The effect is similar to that of ETHlargementPill.
                /// Supported on graphics cards with GDDR5 or GDDR5X memory only.
                /// Requires running the miner with administrative privileges.
                /// Can be set to a comma separated list to apply different values to different cards.
                /// Example: --mt 4 (applies tweak mode #4 to all cards that support this functionality)
                /// --mt 3,3,3,0 (applies tweak mode #3 to all cards except the last one)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_memoryTweak",
                    ShortName = "--mt",
                    Delimiter = ","
                },
                /// <summary>
                /// control hashrate summary report frequency based on the number of share submissions
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_reportInterval",
                    ShortName = "--gpu-report-interval-s"
                },
                /// <summary>
                ///  [Ethash, ProgPOW, Octopus] Controls how DAG is built (default: 0).
                ///  0 - auto (miner will choose the most appropriate mode based on the GPU model)
                ///  1 - default (suitable for most graphics cards)
                ///  2 - recommended for 30xx cards to prevent invalid shares
                ///  Can be set to a comma separated list to apply different values to different cards.
                ///  (eg: --dag-build-mode 1,1,2,1)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_dagBuildMode",
                    ShortName = "--dag-build-mode",
                    Delimiter = ","
                },
                /// <summary>
                /// Executes user script right after miner start (eg: --script-start path_to_user_script)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_scriptStart",
                    ShortName = "--script-start"
                },
                /// <summary>
                /// Executes user script on epoch change.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_scriptEpochChange",
                    ShortName = "--script-epoch-change"
                },
                /// <summary>
                /// Executes user script in case of miner crash.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_scriptCrash",
                    ShortName = "--script-crash"
                },
                /// <summary>
                /// Executes user script in case of low hash. Hash threshold is set in MegaHashes/second.
                /// Example: --script-low-hash script_to_activate:50
                /// (activates "script_to_activate" script once total hashrate drops to 50MH/s)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_scriptLowHash",
                    ShortName = "--script-low-hash"
                },
                /// <summary>
                /// Executes user script right before miner exit.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_scriptExit",
                    ShortName = "--script-exit"
                },
                /// <summary>
                /// [Ethash, Autolykos2] LHR auto-tune mode (default: full). Valid values:
                ///  off  - auto-tune is disabled. LHR tune value is fixed during mining, and will not change
                ///         no matter how often LHR lock is detected
                ///  down - LHR tune value will decrease if the miner detects LHR lock
                ///  full - same as "down" but additionally miner will be trying to increase LHR tune
                ///         value if it's stable on the current LHR tune level
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_lhrautotunelhr",
                    ShortName = null,
                    LongName = "--lhr-autotune-mode",
                    DefaultValue = "off",
                    Delimiter = null
                },
                /// <summary>
                /// [Ethash, Autolykos2] Reduces power consumption in LHR mode at a cost of a slightly lower hashrate.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_lhrlowpower",
                    ShortName = null,
                    LongName = "--lhr-low-power",
                    DefaultValue = null,
                    Delimiter = null
                },
                /// <summary>
                /// [Ethash, Autolykos2] LHR tuning value that indicates the percentage of the full speed the miner
                ///         tries to achieve for LHR cards (default: -1). Range from 10 to 95.
                ///    -1 - auto-mode (LHR tune is set to 71 (or 68 in low power mode) for LHR cards and 0 for non-LHR)
                ///    0 - disabled (use for non-LHR cards)
                ///    30 - recommended starting value for most LHR cards in LHR unlock dual mining mode (see --lhr-algo)
                ///    68 - recommended starting value for most LHR cards in low power mode (see --lhr-low-power)
                ///    71 - recommended starting value for most LHR cards
                ///    Can be set for each GPU separately, e.g.
                ///    "lhr-tune": "0,0,71.5,0" - this will set LHR tuning value to 71.5 for the third GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_lhr",
                    ShortName = null,
                    LongName = "--lhr-tune",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Specifies desired locked GPU core clock speed in MHz. (default: 0 - disabled).
                /// Requires running the miner with administrative privileges.
                /// Example: --lock-cclock 1000 (applies clock 1000Mhz to all cards that support this functionality)
                ///          --lock-cclock 1000,1300,0 (applies clock 1000Mhz to GPU #0, 1300MHz to GPU #1, ignore GPU #2)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_lock_cclock",
                    ShortName = null,
                    LongName = "--lock-cclock",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// Sets GPU core clock offset in MHz.
                /// Requires running the miner with administrative privileges.
                /// Will be set to 0 on exit and during DAG rebuild.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_cclock",
                    ShortName = null,
                    LongName = "--cclock",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Sets GPU memory clock offset in MHz.
                /// Requires running the miner with administrative privileges.
                /// Will be set to 0 on exit and during DAG rebuild.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_mclock",
                    ShortName = null,
                    LongName = "--mclock",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Sets GPU power limit (Windows - in percent, Linux - in Watts)
                /// Requires running the miner with administrative privileges.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_pl",
                    ShortName = null,
                    LongName = "--pl",
                    DefaultValue = "100"
                },
                /// <summary>
                /// All options can be set to a comma separated list to apply different values to
                ///     different cards. (default value for all options: 0 - not used)
                /// Sets GPU fan speed in percent or target temperature (auto-fan).
                /// Valid formats:
                ///      --fan N           (where N is the fan speed)
                ///      --fan t:N         (where N is the target core temperature)
                ///      --fan t:N[F1-F2]  (same as above, but with the fan speed constrained by [F1%, F2%] range)
                ///      --fan tm:N        (where N is the target memory temperature)
                ///      --fan tm:N[F1-F2] (same as above, but with the fan speed constrained by [F1%, F2%] range)
                /// Example: --fan 45,t:67,tm:95,t:69[45-100],tm:90[50-95] which translates to
                ///       GPU #0: set fan speed to 45%
                ///       GPU #1: maintain GPU core temperature at 67C
                ///       GPU #2: maintain GPU memory temperature at 90C
                ///       GPU #3: maintain GPU core temperature at 69C
                ///               with the fan speed limited to [45%, 100%] range
                ///       GPU #4: maintain GPU memory temperature at 90C
                ///               with the fan speed limited to [50%, 95%] range
                /// Note: fan speeds are limited to [0%, 100%] range in auto-fan mode by default.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_fan",
                    ShortName = null,
                    LongName = "--fan",
                    DefaultValue = "0"
                }
            },
            TemperatureOptions = new List<MinerOption>
            {               
                /// <summary>
                /// GPU shutdown temperature. (default: 0 - disabled)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempLimit",
                    LongName = "--temperature-limit",
                    DefaultValue = "0"
                },
                /// <summary>
                /// GPU temperature to enable card after disable. (default: 0 - disabled)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempStart",
                    LongName = "--temperature-start",
                    DefaultValue = "0"
                }
            }
        };
    }
}
