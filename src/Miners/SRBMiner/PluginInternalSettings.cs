using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace SRBMiner
{
    internal static class PluginInternalSettings
    {
        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// (gpu intensity, 1-31 or if > 31 it's treated as raw intensity, separate values with ; and !)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_intensity",
                    ShortName = "--gpu-intensity",
                    Delimiter = "!"
                },
                /// <summary>
                /// (0-disabled, 1-light, 2-normal, separate values with ; and !)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_auto_intensity",
                    ShortName = "--gpu-auto-intensity",
                    Delimiter = "!"
                },
                /// <summary>
                /// number of gpu threads, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_threads",
                    ShortName = "--gpu-threads",
                    Delimiter = "!"
                },
                /// <summary>
                /// gpu worksize, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_worksize",
                    ShortName = "--gpu-worksize",
                    Delimiter = "!"
                },
                /// <summary>
                /// ADL to use (1 or 2), comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_adl_type",
                    ShortName = "--gpu-adl-type",
                    Delimiter = "!"
                },
                /// <summary>
                /// number from 0-10, 0 disables tweaking
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_tweak_profile",
                    ShortName = "--gpu-tweak-profile"
                },
                /// <summary>
                /// use config file other than config.txt
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_config",
                    ShortName = "--config-file"
                },
                /// <summary>
                /// disable gpu tweaking options, which are enabled by default
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_disable_gpu_tweaking",
                    ShortName = "--disable-gpu-tweaking"
                },
                /// <summary>
                /// disable msr extra tweaks, which are enabled by default
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_disable_msr_tweaks",
                    ShortName = "--disable-msr-tweaks"
                },
                /// <summary>
                /// release ocl resources on miner exit/restart
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_opencl_cleanup",
                    ShortName = "--enable-opencl-cleanup"
                },
                /// <summary>
                /// enable workers slow start
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_enable_slow_start",
                    ShortName = "--enable-workers-ramp-up"
                },
                /// <summary>
                /// enable more informative logging
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_log_extended",
                    ShortName = "--extended-log"
                },
                /// <summary>
                /// enable logging to file
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_log_file",
                    ShortName = "--log-file"
                },
                /// <summary>
                /// defines the msr tweaks to use 0-4, | 0 - Intel, 0,1,2,3,4 - AMD
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_msr_tweaks",
                    ShortName = "--msr-use-tweaks"
                },
                /// <summary>
                /// sets AMD gpu's to compute mode & disables crossfire - run as admin
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_set_compute",
                    ShortName = "--set-compute-mode"
                },
                /// <summary>
                /// run custom script on miner start - set clocks, voltage, etc.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_startup_script",
                    ShortName = "--startup-script"
                },
                /// <summary>
                /// number 1-2, try 1 when you need a little bit more free memory for DAG. Default is 2
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_ethash_mode",
                    ShortName = "--gpu-ethash-mode"
                },
                /// <summary>
                /// disable cpu auto affinity setter
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "srbminer_disable_cpu_affinity",
                    ShortName = "--disable-cpu-auto-affinity"
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
                    ID = "srbminer_target_temp",
                    ShortName = "--gpu-target-temperature",
                    Delimiter = "!"
                },
                /// <summary>
                /// gpu turn off temperature, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_off_temp",
                    ShortName = "--gpu-off-temperature",
                    Delimiter = "!"
                },
                /// <summary>
                /// gpu fan speed in RPM, comma separated values
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "srbminer_target_fan",
                    ShortName = "--gpu-target-fan-speed",
                    Delimiter = "!"
                },
                /// <summary>
                /// if this temperature is reached, miner will shutdown system (ADL must be enabled)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_shutdown_temp",
                    ShortName = "--shutdown-temperature"
                },
                /// <summary>
                /// GPU boost
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "srbminer_--gpu-boost",
                    ShortName = "--gpu-boost"
                }
            }
        };
    }
}
