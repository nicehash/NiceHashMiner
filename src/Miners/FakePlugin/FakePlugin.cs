using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FakePlugin
{
    /// <summary>
    /// Plugin class inherits IMinerPlugin interface for registering plugin
    /// </summary>
    public partial class FakePlugin : PluginBase
    {

        //public override string PluginUUID => "12a1dc50-1d9d-11ea-8dad-816592d8b973"; //plugin 1
        //public override string PluginUUID => "1d21d950-1d9d-11ea-8dad-816592d8b973"; //plugin 2
        public override string PluginUUID => "24c913d0-1d9d-11ea-8dad-816592d8b973"; //plugin3
        public override string Name => GetPluginName();
        public override Version Version => GetPluginVersion();

        internal static testSettingsJson DEFAULT_SETTINGS = new testSettingsJson
        {
            name = "FakePlugin1",
            exitTimeWaitSeconds = 5,
            version = new Version(10, 0),
        };

        private testSettingsJson GetTestSettings()
        {
            try
            {
                var path = Path.Combine(Paths.MinerPluginsPath(), PluginUUID, "testSettings.json");
                string text = File.ReadAllText(path);
                var settingsObject = JsonConvert.DeserializeObject<testSettingsJson>(text);
                if (settingsObject != null) return settingsObject;
            }
            catch {}
            return DEFAULT_SETTINGS;
        }


        private string GetPluginName()
        {
            var settingsObject = GetTestSettings();
            return settingsObject.name;
        }

        private Version GetPluginVersion()
        {
            var settingsObject = GetTestSettings();
            return settingsObject.version;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            foreach (var device in devices)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(device);
                if (algorithms.Count > 0) supported.Add(device, algorithms);
            }
            return supported;
        }

        public FakePlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "1.83",
                ExePath = new List<string> { "DemoMiner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/binVer/DemoMiner.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Fake miner - High-performance miner for NVIDIA and AMD GPUs.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string Author => "info@nicehash.com";

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return new List<string>() {};
        }
        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            var settingsObject = GetTestSettings();
            var rebenchAlgos = settingsObject.rebenchmarkAlgorithms;
            if (rebenchAlgos == null) return false;

            var isReBenchVersion = benchmarkedPluginVersion.Major == Version.Major && benchmarkedPluginVersion.Minor < Version.Minor;
            var first = ids.FirstOrDefault();
            var isInList = rebenchAlgos.Contains(first);
            return isReBenchVersion && isInList;
        }
        protected override MinerBase CreateMinerBase()
        {
            return new FakeMiner(PluginUUID);
        }
    }
}
