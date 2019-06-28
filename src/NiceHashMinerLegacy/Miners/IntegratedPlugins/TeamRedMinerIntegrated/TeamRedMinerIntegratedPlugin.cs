using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class TeamRedMinerIntegratedPlugin : TeamRedMiner.TeamRedMinerPlugin, IntegratedPlugin
    {
        public TeamRedMinerIntegratedPlugin() : base("TeamRedMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
