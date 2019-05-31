using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class GMinerIntegratedPlugin : GMinerPlugin.GMinerPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public GMinerIntegratedPlugin() : base("GMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
