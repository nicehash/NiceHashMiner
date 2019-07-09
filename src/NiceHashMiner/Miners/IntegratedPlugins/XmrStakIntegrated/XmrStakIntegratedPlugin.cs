using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class XmrStakIntegratedPlugin : XmrStak.XmrStakPlugin, IntegratedPlugin
    {
        public XmrStakIntegratedPlugin() : base("XmrStak")
        { }

        public bool Is3rdParty => false;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
