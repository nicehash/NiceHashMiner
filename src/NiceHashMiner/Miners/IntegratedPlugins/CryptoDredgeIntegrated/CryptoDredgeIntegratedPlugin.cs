using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class CryptoDredgeIntegratedPlugin : CryptoDredge.CryptoDredgePlugin, IntegratedPlugin
    {
        public override string PluginUUID => "CryptoDredge";

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
