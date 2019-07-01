using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public class TRexIntegratedPlugin : TRex.TRexPlugin, IntegratedPlugin
    {

        public TRexIntegratedPlugin() : base("TRex")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
