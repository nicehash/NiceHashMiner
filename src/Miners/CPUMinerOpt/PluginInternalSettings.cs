using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace CpuMinerOpt
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(200, 5, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout = DefaultTimeout,
        };

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// number of miner threads (default: number of processors)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "cpuminer_threads",
                    ShortName = "-t",
                    LongName = "--threads=",
                },
                /// <summary>
                /// set process priority (default: 0 idle, 2 normal to 5 highest)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "cpuminer_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "0"
                },
                //TODO WARNING this functionality can overlap with already implemented one!!!
                /// <summary>
                /// set process affinity to cpu core(s), mask 0x3 for cores 0 and 1
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "cpuminer_affinity",
                    ShortName = "--cpu-affinity",
                }
            }
        };
    }
}
