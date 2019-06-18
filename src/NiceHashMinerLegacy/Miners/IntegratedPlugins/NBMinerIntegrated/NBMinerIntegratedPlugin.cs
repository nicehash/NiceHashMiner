using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class NBMinerIntegratedPlugin : NBMiner.NBMinerPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public NBMinerIntegratedPlugin() : base("NBMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
