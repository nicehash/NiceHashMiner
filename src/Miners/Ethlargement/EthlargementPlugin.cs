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

namespace Ethlargement
{
    public class Ethlargement : IMinerPlugin, IInitInternals, IBackroundService
    {
        public Ethlargement(string pluginUUID = "efd40691-618c-491a-b328-e7e020bda7a3")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 0);
        public string Name => "Ethlargement";

        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // TODO "register devices and filter it in InitInternals"

            // return empty
            return new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        }

        public bool ServiceEnabled { get; set; } = false;

        public void Start(IEnumerable<MiningPair> miningPairs)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #region IMinerPlugin stubs
        public IMiner CreateMiner()
        {
            return null;
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return false;
        }
        #endregion IMinerPlugin stubs

        #region Internal settings

        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var pluginRootIntenrals = Path.Combine(pluginRoot, "internals");
            var supportedDevicesSettingsPath = Path.Combine(pluginRootIntenrals, "SupportedDevicesSettings.json");
            var fileMinerOptionsPackage = InternalConfigs.ReadFileSettings<SupportedDevicesSettings>(supportedDevicesSettingsPath);
            if (fileMinerOptionsPackage != null && fileMinerOptionsPackage.UseUserSettings)
            {
                _supportedDevicesSettings = fileMinerOptionsPackage;
            }
            else
            {
                InternalConfigs.WriteFileSettings(supportedDevicesSettingsPath, _supportedDevicesSettings);
            }
        }

        protected SupportedDevicesSettings _supportedDevicesSettings = new SupportedDevicesSettings
        {
            SupportedDeviceNames = new List<string> { "1080", "1080 Ti", "Titan Xp" }
        };
        #endregion Internal settings
    }
}
