using MinerPlugin;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class EWBFIntegratedPlugin : EWBF.EwbfPlugin, IntegratedPlugin
    {
        public EWBFIntegratedPlugin() : base("Ewbf")
        { }

        public bool Is3rdParty => true;

        public new IMiner CreateMiner()
        {
            return new EWBFIntegratedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }
    }
}
