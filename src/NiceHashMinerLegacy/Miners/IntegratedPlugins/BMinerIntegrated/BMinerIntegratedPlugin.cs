using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class BMinerIntegratedPlugin : BMiner.BMinerPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public BMinerIntegratedPlugin() : base("BMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
