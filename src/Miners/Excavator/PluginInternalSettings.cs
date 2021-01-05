using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;

namespace Excavator
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 3, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new MinerApiMaxTimeoutSetting { GeneralTimeout = DefaultTimeout };

        internal static MinerBenchmarkTimeSettings BenchmarkTimeSettings = new MinerBenchmarkTimeSettings
        {
            General = new Dictionary<BenchmarkPerformanceType, int> {
                { BenchmarkPerformanceType.Quick,    20  },
                { BenchmarkPerformanceType.Standard, 40  },
                { BenchmarkPerformanceType.Precise,  60  },
            },
        };
    }
}
