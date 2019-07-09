using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class ClaymoreDual14IntegratedPlugin : ClaymoreDual14.ClaymoreDual14Plugin, IntegratedPlugin
    {
        public ClaymoreDual14IntegratedPlugin() : base("ClaymoreDual14+")
        { }

        public bool Is3rdParty => true;

        IEnumerable<string> IntegratedPlugin.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin(PluginUUID);
        }
    }
}
