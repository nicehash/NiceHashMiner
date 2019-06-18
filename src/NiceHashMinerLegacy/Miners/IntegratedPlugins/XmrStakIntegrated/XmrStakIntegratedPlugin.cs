using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class XmrStakIntegratedPlugin : XmrStak.XmrStakPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public XmrStakIntegratedPlugin() : base("XmrStak")
        { }

        public bool Is3rdParty => false;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
