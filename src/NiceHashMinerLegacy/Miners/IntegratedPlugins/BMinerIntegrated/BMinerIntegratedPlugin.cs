using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class BMinerIntegratedPlugin : BMiner.BMinerPlugin, IntegratedPlugin
    {
        public BMinerIntegratedPlugin() : base("BMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
