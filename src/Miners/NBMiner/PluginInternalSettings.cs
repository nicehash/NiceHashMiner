using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
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

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption> { 
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
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_powerOptimize",
                    LongName = "--cuckatoo-power-optimize",
                    DefaultValue = "0"
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
                /// Use 'yyyy-MM-dd HH:mm:ss,zzz' for log time format.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_longTimeFormat",
                    LongName = "--long-format"
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
