using MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class BrokenPluginIntegratedPlugin : BrokenMiner.BrokenMinerPlugin, IntegratedPlugin, IMinerBinsSource
    {
        //public static BrokenPluginIntegratedPlugin Instance { get; } = BrokenMiner.BrokenMinerPluginFactory.Create();
        //BrokenPluginIntegratedPlugin() 
        //{ }
        //BrokenMinerPluginFactory
        public bool Is3rdParty => false;
        IEnumerable<string> IMinerBinsSource.GetMinerBinsUrls()
        {
            return MinersBinsUrls.GetMinerBinsUrlsForPlugin("BrokenMinerPluginUUID");
        }

    }
}
