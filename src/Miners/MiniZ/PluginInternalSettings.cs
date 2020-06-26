using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace MiniZ
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Personalization string for equihash algorithm.
                /// Use auto for automatic personalization string
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_personalization_string",
                    ShortName = "--pers="
                },
                /// <summary>
                /// Lists each GPU information in a different line (default output if running &gt; 1 GPUs).
                /// Omission will print information in the same line.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_gpu_line",
                    ShortName = "--gpu-line"
                },
                /// <summary>
                /// Show number of solutions per iteration.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_show_solration",
                    ShortName = "--show-solratio"
                },
                /// <summary>
                /// Show personalization string.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_show_personalization_string",
                    ShortName = "--show-pers"
                },
                /// <summary>
                /// Show current server ping latency.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_show_latency",
                    ShortName = "--latency"
                },
                /// <summary>
                /// Show accepted/rejected shares per GPU, instead of accepted share efficiency
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_shares_detail",
                    ShortName = "--shares-detail"
                },
                /// <summary>
                /// Include fee shares on statistics
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_all_shares",
                    ShortName = "--all-shares"
                },
                /// <summary>
                /// Show submitted shares / rejected shares
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_show_shares",
                    ShortName = "--show-shares"
                },
                /// <summary>
                /// Lists percentage time connected to each server (yours and fee's)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_server_share",
                    ShortName = "--server-share"
                },
                /// <summary>
                /// Alternative way to specify all the three previous arguments.
                /// This will show: shares + server address + server share
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_show_extra",
                    ShortName = "--extra"
                },
                /// <summary>
                /// Copy program output to miniZ.log
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_log",
                    ShortName = "--log"
                },
                /// <summary>
                /// Copy program output to filename
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_log_file",
                    ShortName = "--logfile"
                },
                /// <summary>
                /// Define the time interval (in seconds) between periodic writes to the log/output file.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_log_period",
                    ShortName = "--log-period"
                },
                /// <summary>
                /// Define the time to wait (in seconds) before starting periodic writes to the log/output file.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_log_delay",
                    ShortName = "--log-delay"
                },
                /// <summary>
                /// Read configuration from [filename] (default: miniZ.conf).
                /// If not specified, miniZ will always run miniZ-master.conf if this file exists.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_read_config",
                    ShortName = "--read-config"
                },
                /// <summary>
                /// Write configuration to [filename] (default: miniZ.conf).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_write_config",
                    ShortName = "--write-config"
                },
                /// <summary>
                /// Combine this option with power saving to mine faster.
                /// Can be defined per GPU. Ex. --oc1=1,4 applies oc1 to GPU 1 and to GPU 4 (numbering as in --cuda-devices).
                /// If GPU is not specified applies to all GPUs.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "miniZ_oc1",
                    ShortName = "--oc1",
                    Delimiter = ","
                },
                /// <summary>
                /// Combine this option with OC to mine faster.
                /// Can be defined per GPU. Ex. --oc2=2,4 applies oc2 to GPU 2 and to GPU 4 (numbering as in --cuda-devices).
                /// If GPU is not specified applies to all GPUs.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "miniZ_oc2",
                    ShortName = "--oc2",
                    Delimiter = ","
                },
                /// <summary>
                /// miniZ will try to find the best kernel for your GPU.
                /// It runs a few available miniZ kernels and chooses the one that performs best.1
                /// It starts with the one set by default, the one we chose to be the best in stock settings.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "miniZ_ocX",
                    ShortName = "--ocX"
                },
                /// <summary>
                /// Force tuning feature 11 level [0-3].
                /// Usually at low power --f11=0 produces better efficiency, but feel free to try different values with your overclock settings.
                /// Ex. --f11=1
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_f11",
                    ShortName = "--f11="
                },
                /// <summary>
                /// Specify miner running intensity, per GPU.
                /// Ex. --intensity=20,40  applies --intensity=20 to the first selcted GPU and --intensity=40 to the second selected GPU.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "miniZ_intensity",
                    ShortName = "--intensity=",
                    Delimiter = ","
                },
                /// <summary>
                /// Adds n% to 2% fee.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_donation",
                    ShortName = "--donate="
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Shows temperature in Celsius (C) or Fahrenheit (F).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_temperature_unit",
                    ShortName = "--tempunits="
                },
                /// <summary>
                /// Temperature limit at which mining will suspend for one minute.
                /// You can specify temperature in Celsius (C) or Fahrenheit (F).
                /// Example: 90C / 194F
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "miniZ_temperature_limit",
                    ShortName = "--templimit="
                }
            }
        };
    }
}
