using MinerPlugin;

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
