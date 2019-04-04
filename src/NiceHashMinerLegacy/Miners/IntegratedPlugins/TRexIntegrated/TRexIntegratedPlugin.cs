using MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public class TRexIntegratedPlugin : TRex.TRexPlugin, IntegratedPlugin
    {

        public TRexIntegratedPlugin() : base("TRex")
        { }

        public bool Is3rdParty => true;

        public new IMiner CreateMiner()
        {
            return new TRexIntegratedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }
    }
}
