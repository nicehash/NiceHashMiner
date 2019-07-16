using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace LolMinerBeam
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// When set to 1 this parameter turns on the usage of assembly tuned kernels.
                /// At the moment lolMiner 0.7.1 only features assembly tuned kernels for mining Beam on RX470 / 480 / 570 / 580, Vega 56 and 64 and Radeon VII
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_activateBinaryKernels",
                    ShortName = "--asm",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Enables (1) or Disables (0) to make the miner write its text output to a log file.
                /// The file will be located in the “logs” directory at the miner location and will be named by the date and time the miner started.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableLogs",
                    ShortName = "--logs",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This two parameters control the length between two statistics show. The longer interval statistics is shown with a blue color, the shorter only black and while.
                /// Setting an interval length of 0 will disable the corresponding statistics output.
                /// Note: disabling the short statistics output will also disable the shortaccept option (see below).
                /// Also the intervals are used for updating the API output.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_longStats",
                    ShortName = "--longstats",
                    DefaultValue = "300"
                },
                /// <summary>
                /// This two parameters control the length between two statistics show. The longer interval statistics is shown with a blue color, the shorter only black and while.
                /// Setting an interval length of 0 will disable the corresponding statistics output.
                /// Note: disabling the short statistics output will also disable the shortaccept option (see below).
                /// Also the intervals are used for updating the API output.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_shortStats",
                    ShortName = "--shortstats",
                    DefaultValue = "30"
                },
                /// <summary>
                /// When setting this parameter to 1, lolMiner will replace the “submitting share / share accepted” message pair by * symbols at the short statistics interval output.
                /// Every star stands for an accepted share.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_compactNotification",
                    ShortName = "--shortaccept",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Setting this parameter to 1 will activate the current daytime to be printed in the command-line console at each statistics output.
                /// This is true for command line as well as the log file when used.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableTimestamps",
                    ShortName = "--timeprint",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This parameter can be used to fix the sol/s output of a GPU to a fixed number of digits after the decimal delimiter.
                /// For example “DIGITS” : 0 will chop of all digits after the decimal delimiter. 
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_decimalDigits",
                    ShortName = "--digits",
                    DefaultValue = "0"
                },
                /// <summary>
                /// This parameter can be used to set a new location for the kernel directory. Absolute path are allowed, so you can freely place it when needed.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_kernelsLocation",
                    ShortName = "--kernelsdir",
                    DefaultValue = "./kernels"
                }
            }
        };
    }
}
