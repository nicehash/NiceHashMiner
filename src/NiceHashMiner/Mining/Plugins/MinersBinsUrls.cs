using MinerPluginToolkitV1.Configs;
using Newtonsoft.Json;
using NHM.Common;
using System.Collections.Generic;
using System.Linq;


// TODO completely remove MinersBinsUrls 
// TODO make an interface for miner plugins 
namespace NiceHashMiner.Mining.Plugins
{
    internal static class MinersBinsUrls
    {
        private static Dictionary<string, List<string>> _pluginsUrls = new Dictionary<string, List<string>>
        {
            //{  "PLUGIN_UUID", new List<string>{ urls... } },
        };

        internal class MinersBinsUrlsSettings
        {
            [JsonProperty("use_file_settings")]
            public bool UseFileSettings { get; set; } = false;

            [JsonProperty("plugin_bins_urls")]
            public Dictionary<string, List<string>> PluginsUrls { get; set; } = null;
        }

        static MinersBinsUrls()
        {
            string binsUrlSettings = Paths.RootPath("miner_bins_urls.json");
            var fileSettings = InternalConfigs.ReadFileSettings<MinersBinsUrlsSettings>(binsUrlSettings);
            if (fileSettings != null && fileSettings.UseFileSettings && fileSettings.PluginsUrls != null)
            {
                _pluginsUrls = fileSettings.PluginsUrls;
            }
            else
            {
                InternalConfigs.WriteFileSettings(binsUrlSettings, new MinersBinsUrlsSettings { PluginsUrls = _pluginsUrls });
            }
        }

        internal static IEnumerable<string> GetMinerBinsUrlsForPlugin(string pluginUUID)
        {
            if (_pluginsUrls.ContainsKey(pluginUUID) && _pluginsUrls[pluginUUID] != null && _pluginsUrls[pluginUUID].Count > 0)
            {
                return _pluginsUrls[pluginUUID];
            }

            return Enumerable.Empty<string>();
        }

        // IMinerBinsSource
        /// <summary>
        /// Return ordered urls where we can download miner binary files
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetMinerBinsUrlsForPlugin(this IntegratedPlugin plugin)
        {
            string pluginUUID = plugin.PluginUUID;
            if (_pluginsUrls.ContainsKey(pluginUUID) && _pluginsUrls[pluginUUID] != null && _pluginsUrls[pluginUUID].Count > 0)
            {
                return _pluginsUrls[pluginUUID];
            }

            return Enumerable.Empty<string>();
        }
    }
}
