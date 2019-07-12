using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class NanoMinerIntegratedPlugin : NanoMiner.NanoMinerPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "NanoMiner";

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
