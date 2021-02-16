using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace TTMiner
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 3, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout = DefaultTimeout,
        };

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
                /// Comma or space separated list of intensities that should be used mining.
			    /// First value for first GPU and so on. A single value sets the same intensity to all GPUs. A value of -1 uses the default intensity of the miner.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_intensity",
                    ShortName = "-i",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid. Same as intensity (-i) just defines the size for the grid directly.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_intensity_grid",
                    ShortName = "-ig",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid-size. This will give you more and finer control about the gridsize.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_grid_size",
                    ShortName = "-gs",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                
                /// <summary>
                /// Reports the current hashrate every 90 seconds to the pool
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ttminer_rate",
                    ShortName = "-RH",
                    LongName = "-rate"
                },
                /// <summary>
                /// This option set the process priority for TT-Miner to a different level:
                /// 1 low
                /// 2 below normal
                /// 3 normal
                /// 4 above normal
                /// 5 high
                /// Default: -PP 3
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ttminer_processPriority",
                    ShortName = "-PP",
                    DefaultValue = "3"
                },
                /// <summary>
                /// Performance-Report GPU-name
                /// Prints the name/model in the performance report
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ttminer_perfRepGpuName",
                    ShortName = "-PRGN",
                },
                /// <summary>
                /// Performance-Report Hash-Rate Interval
                /// Performance-Report & information after INT multiple of one minute. Minimum value for INT to
                /// 1 which creates a hashrate interval of a minute. Higher Intervals gives you a more stable
                /// hashrate. If the interval is too high the displayed average of your hashrate will change
                /// very slowly. The default of 2 will give you an average of 2 minutes.
                /// Default: -PRHRI 2
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ttminer_perfRepHashRateInterval",
                    ShortName = "-PRHRI",
                    DefaultValue = "2"
                },
                /// <summary>
                /// Performance-Report & information after INT multiple of 5 seconds
                /// Set INT to 0 to disable output after a fixed timeframe
                /// sample -RPT 24 shows the performance report after 24 * 5 sec = 2 minutes
                /// Default: -PRT 3
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ttminer_perfRepInfoTime",
                    ShortName = "-PRT",
                    DefaultValue = "3"
                },
                /// <summary>
                /// Performance-Report & information after a INT shares found
                /// Set INT to 0 to disable output after a fixed number of shares
                /// sample - RPS 10 shows the performance report after 10 shares were found
                /// Default: -PRS 0
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ttminer_perfRepInfoShares",
                    ShortName = "-PRS",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Enable logging of the pool communication. TT-Miner creates the pool-logfile in the folder 'Logs'.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ttminer_logPool",
                    ShortName = "-logpool",
                },
                /// <summary>
                /// Enable logging of screen output and additional information, the file is created in the folder 'Logs'.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ttminer_log",
                    ShortName = "-log",
                },
            }
        };
    }
}
