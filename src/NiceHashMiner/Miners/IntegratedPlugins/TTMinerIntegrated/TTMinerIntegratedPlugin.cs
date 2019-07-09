using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public class TTMinerIntegratedPlugin : TTMiner.TTMinerPlugin, IntegratedPlugin
    {
        public TTMinerIntegratedPlugin() : base("TTMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
