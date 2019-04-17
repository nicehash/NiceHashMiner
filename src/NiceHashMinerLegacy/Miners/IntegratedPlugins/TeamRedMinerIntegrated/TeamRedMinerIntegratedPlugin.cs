using MinerPlugin;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class TeamRedMinerIntegratedPlugin : TeamRedMiner.TeamRedMinerPlugin, IntegratedPlugin
    {
        public TeamRedMinerIntegratedPlugin() : base("TeamRedMiner")
        { }

        public bool Is3rdParty => true;
    }
}
