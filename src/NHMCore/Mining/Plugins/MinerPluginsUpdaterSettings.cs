using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Configs;
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

        static readonly SupportedAlgorithmsFilterSettingsFile _settings;

        static MinerPluginsUpdaterSettings()
        {
            (_settings, _) = InternalConfigs.GetDefaultOrFileSettings(Paths.InternalsPath("MinerPluginsUpdaterSettings.json"), new SupportedAlgorithmsFilterSettingsFile());
        }

        public static TimeSpan CheckPluginsInterval => _settings.CheckPluginsInterval;
    }
}
