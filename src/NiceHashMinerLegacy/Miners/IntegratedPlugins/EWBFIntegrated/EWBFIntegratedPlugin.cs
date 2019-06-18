using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class EWBFIntegratedPlugin : EWBF.EwbfPlugin, IntegratedPlugin, IMinerBinsSource
    {
        public EWBFIntegratedPlugin() : base("Ewbf")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
