using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class TeamRedMinerIntegratedPlugin : TeamRedMiner.TeamRedMinerPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public TeamRedMinerIntegratedPlugin() : base("TeamRedMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
