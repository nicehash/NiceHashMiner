using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public class TTMinerIntegratedPlugin : TTMiner.TTMinerPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public TTMinerIntegratedPlugin() : base("TTMiner")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
