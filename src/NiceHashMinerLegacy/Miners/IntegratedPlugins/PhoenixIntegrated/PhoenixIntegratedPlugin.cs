using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class PhoenixIntegratedPlugin : Phoenix.PhoenixPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public PhoenixIntegratedPlugin() : base("Phoenix")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
