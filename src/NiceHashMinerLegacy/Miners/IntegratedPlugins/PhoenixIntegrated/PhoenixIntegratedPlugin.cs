using MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class PhoenixIntegratedPlugin : Phoenix.PhoenixPlugin, IntegratedPlugin
    {
        public PhoenixIntegratedPlugin() : base("Phoenix")
        { }

        public bool Is3rdParty => true;

        public new IMiner CreateMiner()
        {
            return new PhoenixIntegratedMiner(PluginUUID, _mappedIDs)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }
    }
}
