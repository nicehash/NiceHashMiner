using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class NanoMinerIntegratedPlugin : NanoMiner.NanoMinerPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public NanoMinerIntegratedPlugin() : base("NanoMiner") { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
