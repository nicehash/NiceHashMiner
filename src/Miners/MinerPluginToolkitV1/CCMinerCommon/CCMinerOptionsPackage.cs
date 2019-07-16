using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.CCMinerCommon
{
    public static class CCMinerOptionsPackage
    {
        public static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU threads per call 8-25 (2^N + F, default: 0=auto). Decimals and multiple values are allowed for fine tuning
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ccminertpruvot_intensity",
                    ShortName = "-i",
                    LongName = "--intensity=",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// number of miner threads (default: number of nVidia GPUs in your system)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_threads",
                    ShortName = "-t",
                    LongName = "--threads=",
                }, 
                /// <summary>
                /// Set device threads scheduling mode (default: auto)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ccminertpruvot_cuda_schedule",
                    ShortName = "--cuda-schedule",
                },
                /// <summary>
                /// set process priority (default: 0 idle, 2 normal to 5 highest)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "0",
                },
                /// <summary>
                /// set process affinity to specific cpu core(s) mask
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_affinity",
                    ShortName = "--cpu-affinity",
                }
            }
        };
    }
}
