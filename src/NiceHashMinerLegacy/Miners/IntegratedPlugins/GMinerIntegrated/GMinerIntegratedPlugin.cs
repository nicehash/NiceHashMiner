using MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins.GMinerIntegrated
{
    class GMinerIntegratedPlugin : GMinerPlugin.GMinerPlugin, IntegratedPlugin
    {
        public GMinerIntegratedPlugin() : base("GMiner")
        { }

        public bool Is3rdParty => true;

        public new IMiner CreateMiner()
        {
            return new GMinerIntegratedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }
    }
}
