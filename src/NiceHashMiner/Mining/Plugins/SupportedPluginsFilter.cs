using MinerPluginToolkitV1.Configs;
using NHM.Common;
using System.Collections.Generic;

namespace NiceHashMiner.Mining.Plugins
{
    internal static class SupportedPluginsFilter
    {
        static List<string> _filteredPlugins = new List<string> {
            (new BMinerIntegratedPlugin()).PluginUUID,
        };

        static SupportedPluginsFilter()
        {
            string internalSettingFilePath = Paths.InternalsPath("SupportedPluginsFilter.json");
            var internalSettings = InternalConfigs.ReadFileSettings<List<string>>(internalSettingFilePath);
            if (internalSettings != null)
            {
                _filteredPlugins = internalSettings;
            }
            else
            {
                InternalConfigs.WriteFileSettings(internalSettingFilePath, _filteredPlugins);
            }
        }

        static public bool IsSupported(string pluginUUID)
        {
            var isSupported = !_filteredPlugins.Contains(pluginUUID);
            return isSupported;
        }
    }
}
