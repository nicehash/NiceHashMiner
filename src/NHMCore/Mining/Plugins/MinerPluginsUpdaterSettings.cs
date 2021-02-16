using Newtonsoft.Json;
using NHM.Common;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;

namespace NHMCore.Mining.Plugins
{
    internal static class MinerPluginsUpdaterSettings
    {
        private class SupportedAlgorithmsFilterSettingsFile : IInternalSetting
        {
            [JsonProperty("use_user_settings")]
            public bool UseUserSettings { get; set; } = false;

            [JsonProperty("check_plugins_interval")]
            public TimeSpan CheckPluginsInterval = TimeSpan.FromMinutes(30);
        }

        static SupportedAlgorithmsFilterSettingsFile _settings = new SupportedAlgorithmsFilterSettingsFile();

        static MinerPluginsUpdaterSettings()
        {
            var fileSettings = InternalConfigs.InitInternalSetting(Paths.Root, _settings, "MinerPluginsUpdaterSettings.json");
            if (fileSettings != null) _settings = fileSettings;
        }

        public static TimeSpan CheckPluginsInterval => _settings.CheckPluginsInterval;
    }
}
