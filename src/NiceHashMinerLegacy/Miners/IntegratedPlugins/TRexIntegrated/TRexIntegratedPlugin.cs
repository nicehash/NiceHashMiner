using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public class TRexIntegratedPlugin : TRex.TRexPlugin, IntegratedPlugin, IMinerBinsSource
    {

        public TRexIntegratedPlugin() : base("TRex")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
