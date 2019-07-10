using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class NBMinerIntegratedPlugin : NBMiner.NBMinerPlugin, IntegratedPlugin
    {
        public NBMinerIntegratedPlugin() : base("NBMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
