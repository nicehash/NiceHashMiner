using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class PhoenixIntegratedPlugin : Phoenix.PhoenixPlugin, IntegratedPlugin
    {
        public PhoenixIntegratedPlugin() : base("Phoenix")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
