using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace WildRig
{
    internal static class PluginInternalSettings
    {
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
                /// strategy of feeding videocards with job (default: 0)
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_strategy",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--strategy=",
                    DefaultValue = "0"
                },
                /// <summary>
                /// amount of threads per OpenCL device
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_threads",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-threads=",
                },
                /// <summary>
                /// list of launch config, intensity and worksize
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_launch",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-launch=",
                },
                /// <summary>
                /// affine GPU threads to a CPU
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_affinity",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-affinity=",
                },
                /// <summary>
                /// log all output to a file
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_log",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ShortName = "-l",
                    LongName = "--log-file=",
                },
                /// <summary>
                /// print hashrate report every N seconds
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printTime",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--print-time=",
                },
                /// <summary>
                /// print hashrate for each videocard
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printFull",
                    Type = MinerOptionType.OptionIsParameter,
                    LongName = "--print-full",
                },
                /// <summary>
                /// print additional statistics
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printStatistics",
                    Type = MinerOptionType.OptionIsParameter,
                    LongName = "--print-statistics",
                },
                /// <summary>
                /// print debug information
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printDebug",
                    Type = MinerOptionType.OptionIsParameter,
                    LongName = "--print-debug",
                },
                /// <summary>
                /// print power consumption per GPU chip
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_printPower",
                    Type = MinerOptionType.OptionIsParameter,
                    LongName = "--print-power",
                },
                /// <summary>
                /// donate level, default 2% (2 minutes in 100 minutes)
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_fee",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--donate-level=",
                    DefaultValue = "2",
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// set temperature at which gpu will stop mining(default: 85)
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_tempLimit",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--gpu-temp-limit=",
                    DefaultValue = "85",
                },
                /// <summary>
                /// set temperature at which gpu will resume mining(default: 60)
                /// </summary>
                new MinerOption
                {
                    ID = "wildrig_tempResume",
                    Type = MinerOptionType.OptionWithSingleParameter,
                    LongName = "--gpu-temp-resume=",
                    DefaultValue = "60",
                },
            }
        };
    }
}
