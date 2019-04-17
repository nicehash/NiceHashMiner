using MinerPlugin;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class NBMinerIntegratedPlugin : NBMiner.NBMinerPlugin, IntegratedPlugin
    {
        public NBMinerIntegratedPlugin() : base("NBMiner")
        { }

        public bool Is3rdParty => true;
    }
}
