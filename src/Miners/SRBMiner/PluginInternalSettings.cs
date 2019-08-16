using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRBMiner
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// gpu intensity, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_intensity",
                    ShortName = "--cgpuintensity",
                    Delimiter = ","
                },
                /// <summary>
                /// number of gpu threads, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_threads",
                    ShortName = "--cgputhreads",
                    Delimiter = ","
                },
                /// <summary>
                /// gpu worksize, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_work_size",
                    ShortName = "--cgpuworksize",
                    Delimiter = ","
                },
                /// <summary>
                /// can be 0,1,2,4,8,16,32,64,128, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_fragments",
                    ShortName = "--cgpufragments",
                    Delimiter = ","
                },
                /// <summary>
                /// mode for heavy algos (1, 2, 3), comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_heavy_mode",
                    ShortName = "--cgpuheavymode",
                    Delimiter = ","
                },
                /// <summary>
                /// delay to maintain between same gpu threads, 1 - 1000, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_thread_delay",
                    ShortName = "--cgputhreaddelay",
                    Delimiter = ","
                },
                /// <summary>
                /// gpu adl to use (1 or 2), comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_adl_type",
                    ShortName = "--cgpuadltype",
                    Delimiter = ","
                },
                /// <summary>
                /// old kernel creation mode - true or false, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_old_mode",
                    ShortName = "--cgpuoldmode",
                    Delimiter = ","
                },
                /// <summary>
                /// number from 0-10, where 0 means don't use tweaking
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_tweak_profile",
                    ShortName = "--cgputweakprofile",
                    Delimiter = ","
                },
                /// <summary>
                /// disable gpu tweaking options, which are enabled by default
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_disable_tweaking",
                    ShortName = "--disabletweaking",
                },
                /// <summary>
                /// enable gpu slow start
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_gpu_rampup",
                    ShortName = "--enablegpurampup",
                },
                /// <summary>
                /// --logfile filename (enable logging to file)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_log",
                    ShortName = "--logfile",
                },
                /// <summary>
                /// don't save compiled binaries to disk
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_no_cache",
                    ShortName = "--nocache",
                },
                /// <summary>
                /// how many blocks to precompile for CN/R, min. 3 max. 300. Def. is 15
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_precompile_blocks",
                    ShortName = "--precompileblocks",
                },
                /// <summary>
                /// do some precalculations that *may* increase hashing speed a little bit on weak gpu's
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_prepare_data",
                    ShortName = "--preparedata",
                },
                /// <summary>
                /// reset fans back to default settings on miner exit
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_reset_fans",
                    ShortName = "--resetfans",
                },
                /// <summary>
                /// sets AMD gpu's to compute mode & disables crossfire - run as admin
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_compute_mode",
                    ShortName = "--setcomputemode",
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// gpu temperature, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_target_temp",
                    ShortName = "--cgputargettemperature",
                    Delimiter = ","
                },
                /// <summary>
                /// gpu fan speed in RPM, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_target_fan_speed",
                    ShortName = "--cgputargetfanspeed",
                    Delimiter = ","
                },
                /// <summary>
                /// gpu turn off temperature, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_gpu_off_temp",
                    ShortName = "--cgpuofftemperature",
                    Delimiter = ","
                }
            }
        };
    }
}
