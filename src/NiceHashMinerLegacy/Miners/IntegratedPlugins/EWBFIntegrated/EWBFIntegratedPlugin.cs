using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class EWBFIntegratedPlugin : EWBF.EwbfPlugin, IntegratedPlugin
    {
        public EWBFIntegratedPlugin() : base("Ewbf")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
