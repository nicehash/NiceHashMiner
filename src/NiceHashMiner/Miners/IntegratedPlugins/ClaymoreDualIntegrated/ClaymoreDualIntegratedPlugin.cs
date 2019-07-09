using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class ClaymoreDualIntegratedPlugin : ClaymoreDual.ClaymoreDualPlugin, IntegratedPlugin
    {
        public ClaymoreDualIntegratedPlugin() : base("ClaymoreDual")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
