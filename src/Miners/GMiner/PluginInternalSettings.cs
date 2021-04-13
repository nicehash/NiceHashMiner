using NHM.Common.Enums;
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
                /// space-separated list of OC modes for each device
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_oc",
                    LongName = "--oc",
                    Delimiter = " "
                },
                /// <summary>
                /// enable OC1 for all devices
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "gminer_oc1",
                    LongName = "--oc1"
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
                /// <summary>
                /// improved DAG generation, now miner generates valid DAG in extremal OC modes.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_memory_tweaks",
                    LongName = "--tfan",
                },
            }
        };
    }
}
