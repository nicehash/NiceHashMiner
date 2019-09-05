using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using NHM.Common;
using System.Collections.Generic;

namespace NiceHashMiner.Mining.Plugins
{
    internal static class SupportedPluginsFilter
    {
        private class SupportedPluginsFilterSettings : IInternalSetting
        {
            [JsonProperty("use_user_settings")]
            public bool UseUserSettings { get; set; } = false;

            [JsonProperty("filtered_plugins")]
            public List<string> FilteredPlugins = new List<string> {};
        }

        static SupportedPluginsFilterSettings _settings = new SupportedPluginsFilterSettings { };

        static SupportedPluginsFilter()
        {
            var fileSettings = InternalConfigs.InitInternalSetting(Paths.Root, _settings, "SupportedPluginsFilter.json");
            if (fileSettings != null) _settings = fileSettings;
        }

        static public bool IsSupported(string pluginUUID)
        {
            var isSupported = !_settings.FilteredPlugins.Contains(pluginUUID);
            return isSupported;
        }
    }
}
