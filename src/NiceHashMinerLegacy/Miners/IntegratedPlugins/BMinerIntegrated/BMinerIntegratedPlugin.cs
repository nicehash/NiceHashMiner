using MinerPlugin;


namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class BMinerIntegratedPlugin : BMiner.BMinerPlugin, IntegratedPlugin
    {
        public BMinerIntegratedPlugin() : base("BMiner")
        { }

        public bool Is3rdParty => true;

        public new IMiner CreateMiner()
        {
            return new BMinerIntegratedMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }
    }
}
