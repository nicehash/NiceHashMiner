using MinerPlugin;
using NiceHashMinerLegacy.Common.Device;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class XmrStakIntegratedPlugin : XmrStak.XmrStakPlugin, IntegratedPlugin
    {
        public XmrStakIntegratedPlugin() : base("XmrStak")
        { }

        public bool Is3rdParty => false;
    }
}
