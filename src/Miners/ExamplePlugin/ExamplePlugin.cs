using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    /// <summary>
    /// In plugin class we set all settings used by miner (for example pluginUUID, GetSupportedAlgorithms, Internal Settings and more)
    /// Interfaces from MinerPlugin and MinerPluginToolkitV1 provide large set of settings available for use
    /// </summary>
    public class ExamplePlugin : IMinerPlugin
    {
        public Version Version => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Author => throw new NotImplementedException();

        public string PluginUUID => throw new NotImplementedException();

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            throw new NotImplementedException();
        }

        public IMiner CreateMiner()
        {
            throw new NotImplementedException();
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            throw new NotImplementedException();
        }
    }
}
