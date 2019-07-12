using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace EWBF
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Personalization for equihash, string 8 characters
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_personalization_equihash",
                    ShortName = "--pers"
                },
                /// <summary>
                /// The developer fee in percent allowed decimals for example 0, 1, 2.5, 1.5 etc.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_developer_fee",
                    ShortName = "--fee"
                },
                /// <summary>
                /// Exit in case of error. Value 1 exit if miner cannot restart workers.
                /// Value 2 if lost connection with the pool. 3 both cases.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_eexit",
                    ShortName = "--eexit"
                },
                /// <summary>
                /// Create file miner.log in directory of miner.
                /// Allowed values 1 and 2. 1 only errors, 2 will repeat console output.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_log",
                    ShortName = "--log"
                },
                /// <summary>
                /// Set custom filename.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_logFile",
                    ShortName = "--logfile"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ewbf_solver",
                    ShortName = "--solver",
                    DefaultValue = "0",
                    Delimiter = " "
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ewbf_intensity",
                    ShortName = "--intensity",
                    DefaultValue = "64",
                    Delimiter = " "
                },
                /// <summary>
                /// Power efficiency calculator. Shows power statistics.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ewbf_powercalc",
                    ShortName = "--pec"
                }
            },
            TemperatureOptions = new List<MinerOption>{
                /// <summary>
                /// Temperature limit, gpu will be stopped if this limit is reached.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_templimit",
                    ShortName = "--templimit",
                    DefaultValue = "90"
                },
                /// <summary>
                /// Temperature units, allowed values: C for celsius, F for fahrenheit and K for kelvin).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ewbf_tempunits",
                    ShortName = "--tempunits",
                    DefaultValue = "C"
                }
            }
        };
    }
}
