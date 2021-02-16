using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace ZEnemy
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 3, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new MinerApiMaxTimeoutSetting { GeneralTimeout = DefaultTimeout };

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
                /// GPU intensity 8.0-31.0, decimals allowed (default: 19)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "zenemy_intensity",
                    ShortName = "-i",
                    LongName = "--intensity=",
                    DefaultValue = "19",
                    Delimiter = ","
                },
                /// <summary>
                /// set CUDA scheduling option:
                /// 0: BlockingSync (default)
                /// 1: Spin
                /// 2: Yield
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "zenemy_cudaSchedula",
                    LongName = "--cuda-schedule",
                    DefaultValue = "0",
                },
                /// <summary>
                /// set process priority (default: 3) 0 idle, 2 normal to 5 highest
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "zenemy_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "3"
                },
                //WARNING this functionality can overlap with already implemented one!!!
                /// <summary>
                /// set process affinity to cpu core(s), mask 0x3 for cores 0 and 1
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "zenemy_affinity",
                    ShortName = "--cpu-affinity",
                },
                /// <summary>
                /// disable colored output
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "zenemy_no_color",
                    ShortName = "--no-color",
                },
                /// <summary>
                /// disable NVML hardware sampling
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "zenemy_no_nvml",
                    ShortName = "--no-nvml",
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Only mine if gpu temperature is less than specified value
                /// Can be tuned with --resume-temp=N to set a resume value
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "zenemy_maxTemperature",
                    ShortName = "--max-temp=",
                },
                /// <summary>
                /// resume value for miners to start again after shutdown
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "zenemy_resumeTemperature",
                    ShortName = "--resume-temp=",
                }
            }
        };
    }
}
