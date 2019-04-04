using MinerPlugin;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public class TTMinerIntegratedPlugin : TTMiner.TTMinerPlugin, IntegratedPlugin
    {
        public TTMinerIntegratedPlugin() : base("TTMiner")
        { }

        public bool Is3rdParty => true;

        public new IMiner CreateMiner()
        {
            return new TTMinerIntegratedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }
    }
}
