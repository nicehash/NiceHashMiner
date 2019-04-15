using MinerPlugin;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.ClaymoreCommon;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    abstract class ClaymorePluginBase : IMinerPlugin, IInitInternals, IntegratedPlugin
    {
        public bool Is3rdParty => false;

        public abstract string PluginUUID { get; }

        public abstract Version Version { get; }

        public abstract string Name { get; }

        public string Author => "Domen Kirn Krefl";

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public abstract IMiner CreateMiner();

        // TODO add ELP internal configs here 
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {DefaultSystemEnvironmentVariables = new Dictionary<string, string>(){}};

        protected static MinerOptionsPackage _minerOptionsPackage = ClaymoreBase.DefaultMinerOptionsPackage;

    }

}
