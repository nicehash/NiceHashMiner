using MinerPluginToolkitV1.CCMinerCommon;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using System;

namespace CCMinerMTP
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 2, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new MinerApiMaxTimeoutSetting { GeneralTimeout = DefaultTimeout };

        internal static MinerOptionsPackage MinerOptionsPackage = CCMinerOptionsPackage.MinerOptionsPackage;
    }
}
