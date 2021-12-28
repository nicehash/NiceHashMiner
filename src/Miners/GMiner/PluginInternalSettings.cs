﻿using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace GMinerPlugin
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(1, 15, 0);

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
                /// option to control GPU intensity (--intensity, 1-100)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    Delimiter = " "
                },
                /// <summary>
                /// log filename
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_logfile",
                    ShortName = "-l",
                    LongName = "--logfile"
                },
                /// <summary>
                /// enable/disable color output
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_color",
                    ShortName = "-c",
                    LongName = "--color"
                },
                /// <summary>
                /// personalization string for equihash algorithm (for example: 'BgoldPoW', 'BitcoinZ', 'Safecoin')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_pers",
                    LongName = "--pers",
                },
                /// <summary>
                /// enable/disable power efficiency calculator. Power efficiency calculator display of energy efficiency statistics of GPU in S/w, higher CPU load. Default value is '1' ('0' - off or '1' - on)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_pec",
                    LongName = "--pec=",
                    DefaultValue = "1"
                },
                /// <summary>
                /// enable/disable NVML
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_nvml",
                    LongName = "--nvml",
                    DefaultValue = "1"
                },
                /// <summary>
                /// enable/disable CUDA platform
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_cuda",
                    LongName = "--cuda",
                },
                /// <summary>
                /// enable/disable OpenCL platform
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_opencl",
                    LongName = "--opencl",
                },
                /// <summary>
                /// pass cost of electricity in USD per kWh, miner will report $ spent to mining
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_electricity",
                    LongName = "--electricity_cost"
                },
                /// <summary>
                /// control hashrate report interval
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_reportInterval",
                    LongName = "--report_interval"
                },
                /// <summary>
                /// space-separated list of Dag file modes (0 - auto, 1 - single, 2 - double), separated by spaces, can be empty, default is '0' (for example: '2 1 0')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_dagMode",
                    LongName = "--dag_mode",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of Dag file size limits in megabytes, separated by spaces, can be empty (for example: '4096 4096 4096')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_dagLimit",
                    LongName = "--dag_limit",
                    Delimiter = " "
                },
                /// <summary>
                /// memory tweaks for Nvidia GPUs with GDDR5X and GDDR5 memory, requires admin privileges (--mt 1-6)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_memory_tweaks",
                    LongName = "--mt",
                },
                /// <summary>
                /// improved DAG generation, now miner generates valid DAG in extremal OC modes.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "gminer_--safe_dag",
                    LongName = "--safe_dag",
                },
                /// <summary>
                /// log date
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "gminer_memory_tweaks",
                    LongName = "--log_date",
                },
                /// <summary>
                /// log stratum
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "gminer_--log_stratum",
                    LongName = "--log_stratum",
                },
                /// <summary>
                /// space-separated list of fan speed for each device in percents (range from 0 to 100, 0 - ignore), only Windows is supported (for example: '60 0 90')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_fan",
                    LongName = "--fan",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of power limits for each device in percents (range from 0 to 100 for Nvidia GPUs and -50 - 50 for AMD GPUs, 0 - ignore), only Windows is supported (for example: '30 0 50')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_pl",
                    LongName = "--pl",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of core clock offsets (for Nvidia GPUs) or absolute core clocks (for AMD GPUs) for each device in MHz (0 - ignore),
                /// only Windows is supported, requires running miner with admin privileges (for example: '100 0 -90')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_cclock",
                    LongName = "--cclock",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of memory clock offsets (for Nvidia GPUs) or absolute memory clocks (for AMD GPUs) for each device in MHz (0 - ignore),
                /// only Windows is supported, requires running miner with admin privileges (for example: '100 0 -90')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_mclock",
                    LongName = "--mclock",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of core voltage offsets in % (for Nvidia GPUs) or absolute core voltages (for AMD GPUs) for each device in mV (0 - ignore),
                /// only Windows is supported, requires running miner with admin privileges (for example: '900 0 1100')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_cvddc",
                    LongName = "--cvddc",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of locked voltage points for each device in mV (0 - ignore),
                /// only Windows and Nvidia GPUs are supported. Requires running miner with admin privileges (for example: '900 0 1000')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_lock_voltage",
                    LongName = "--lock_voltage",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of locked core clock point for each device in MHz (0 - ignore), only Nvidia GPUs are supported. 
                /// Requires running miner with admin privileges (for example: '1200 0 1500')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_lock_cclock",
                    LongName = "--lock_cclock",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// enable/disable P2 state, only Windows and Nvidia GPUs are supported. Requires running miner with admin privileges
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_p2state",
                    LongName = "--p2state",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of target temperatures for fan (0 - ignore), only Windows is supported (for example: '65 0 70')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_tfan",
                    LongName = "--tfan",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of minimal fan speed (0 - ignore) for tfan option, only Windows is supported (for example: '30 0 35')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_tfan_min",
                    LongName = "--tfan_min",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of maximal fan speed (0 - ignore) for tfan option, only Windows is supported (for example: '90 0 80')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_tfan_max",
                    LongName = "--tfan_max",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of LHR modes (0 - auto, 1 - on, 2 - off), only Nvidia GPUs are supported
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_lhr",
                    LongName = "--lhr",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of LHR tune values, meaning GPU unlock percentage (0 - auto), only Nvidia GPUs are supported, default value is '0' (for example: '72 71 73')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_lhr_tune",
                    LongName = "--lhr_tune",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of LHR auto-tune, 0 - off, 1 - on, only Nvidia GPUs are supported (for example: '1 0 1')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_lhr_autotune",
                    LongName = "--lhr_autotune",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// LHR auto-tune step size, only Nvidia GPUs are supported, default value is '0.5' (for example: '0.2')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_lhr_autotune_step",
                    LongName = "--lhr_autotune_step",
                    Delimiter = " ",
                    DefaultValue = "0.5"
                },
                /// <summary>
                /// space-separated list of LHR mode (0 - power save mode, 1 - maximal performance mode), only Nvidia GPUs are supported, default value is '1' (for example: '1 0 1')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_lhr_mode",
                    LongName = "--lhr_mode",
                    Delimiter = " ",
                    DefaultValue = "1"
                },
            },
            TemperatureOptions = new List<MinerOption>{
                /// <summary>
                /// space-separated list of temperature limits, upon reaching the limit, the GPU stops mining until it cools down, can be empty (for example: '85 80 75')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_templimit",
                    ShortName = "-t",
                    LongName = "--templimit",
                    DefaultValue = "90",
                    Delimiter = " "
                },
            }
        };
    }
}
