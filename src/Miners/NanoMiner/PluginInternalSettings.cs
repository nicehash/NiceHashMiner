using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace NanoMiner
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
                /// This is the minimum acceptable hashrate. This function keeps track of the rig’s total hashrate and compares it with this parameter.
                /// If five minutes after the miner is launched the set minimum is not reached, nanominer will automatically restart.
                /// Likewise, the miner will restart if for any reason the average hashrate over a ten-minute period falls below the set value.
                /// This value can be set with an optional modifier letter that represents a thousand for kilohash or a million for megahash per second.
                /// For example, setting the value to 100 megahashes per second can be written as 100M, 100.0M, 100m, 100000k, 100000K or 100000000.
                /// If this parameter is not defined, the miner will not restart.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nanominer_minHash",
                    ShortName = "minHashrate="
                },
                /// <summary>
                /// This parameter accepts the values true or false (the default is false). If this parameter is set to true then no log files will be recorded onto the hard drive.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nanominer_noLog",
                    ShortName = "noLog=",
                    DefaultValue = "false"
                },
                /// <summary>
                /// This parameter can either be used to set the name of the folder in which log files will be created (e.g. logPath=logfolder/),
                /// or to specify a path to single file, which will be used for all logs (e.g. logPath=logs/log.txt, logPath=/var/log/nanominer/log.txt, logPath=C:\logs\log.txt).
                /// Both relative and absolute paths work. Default value for this parameter is logs/.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nanominer_logPath",
                    ShortName = "logPath=",
                    DefaultValue = "logs/"
                },
                /// <summary>
                /// Can be set to modify AMD GPU timings on the fly for Ethash algorithm.
                /// The following AMD ASICs are currently supported: gfx900, gfx901, gfx906, gfx907, Baffin, Ellesmere, gfx804, Hawaii, Tahiti, Pitcairn, Tonga.
                /// Default memory tweak value is 1 which means slightly improving memory timings. Zero value means timings are left as is without modifications.
                /// Parameter values must be separated by a comma or space (first value is for GPU0, second is for GPU1, and so on).
                /// Supported memory tweak value range is from 0 to 10 (0 means disabling timings modification, 1 is the least intense, 10 is the most intense).
                /// You can also apply same settings for each GPU by defining only one memory tweak value.
                /// Miner must be launched using admin/root privileges in order to change timings.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nanominer_memTweak",
                    ShortName = "memTweak=",
                    DefaultValue = "1",
                    Delimiter = ","
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Can be used to overclock/underclock NVIDIA GPU’s. Absolute (e.g. 4200) as well as relative (e.g. +200, -150) values in megabytes are accepted.
                /// The values must be separated by a comma or space (first value is for GPU0, second is for GPU1, and so on). For example, if it is set as
                /// coreClocks=+200,-150
                /// memClocks = +300,3900
                /// then GPU0 will be overclocked by 200 MHz of core and 300 MHz of memory, whereas GPU1 core clock will be underclocked by 150 MHz, and its memory clock set to 3900 MHz.
                /// You can also apply same settings for each GPU by defining only one of the core and memory clock values, for example:
                /// coreClocks=+200
                /// memClocks = +300
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nanominer_coreClocks",
                    ShortName = "coreClocks=",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nanominer_memClocks",
                    ShortName = "memClocks=",
                    Delimiter = ","
                }
            }
        };
    }
}
