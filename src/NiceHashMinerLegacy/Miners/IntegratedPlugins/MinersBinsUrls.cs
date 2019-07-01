using MinerPluginToolkitV1.Configs;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    internal static class MinersBinsUrls
    {

        private static Dictionary<string, List<string>> _pluginsUrls = new Dictionary<string, List<string>>
        {
            //{  "BrokenMinerPluginUUID", new List<string>{ "set new URL" } },
            {  "BMiner", new List<string>{ "https://www.bminercontent.com/releases/bminer-lite-v15.5.3-747d98e-amd64.zip" } },
            {  "CCMinerMTP", new List<string>{ "https://github.com/nicehash/ccminer/releases/download/1.1.14/ccminer_mtp.7z" } },
            {  "CCMinerTpruvot", new List<string>{ "https://github.com/tpruvot/ccminer/releases/download/2.3.1-tpruvot/ccminer-2.3.1-cuda10.7z" } },
            {  "ClaymoreDual", new List<string>{ "https://mega.nz/#F!O4YA2JgD!n2b4iSHQDruEsYUvTQP5_w?2sBXjCTY" } },
            {  "ClaymoreDual14+", new List<string>{ "https://mega.nz/#F!O4YA2JgD!n2b4iSHQDruEsYUvTQP5_w?ngYyTARY" } },
            {  "Ethlargement", new List<string>{ "https://github.com/nicehash/NiceHashMinerTest/releases/download/1.9.1.5/Ethlargement.7z" } }, // TODO change to original link
            {  "Ewbf", new List<string>{ "https://mega.nz/#F!fsAlmZQS!CwVgFfBDduQI-CbwVkUEpQ?Tlp22YKT" } },
            {  "GMiner", new List<string>{ "https://github.com/develsoftware/GMinerRelease/releases/download/1.47/gminer_1_47_windows64.zip" } },
            {  "NBMiner", new List<string>{ "https://github.com/NebuTech/NBMiner/releases/download/v23.3/NBMiner_23.3_Win.zip" } },
            {  "Phoenix", new List<string>{ "https://mega.nz/#F!2VskDJrI!lsQsz1CdDe8x5cH3L8QaBw?6UV1FQSZ" } },
            {  "SGminerAvemore", new List<string>{ "https://github.com/brian112358/avermore-miner/releases/download/v1.4.1/avermore-v1.4.1-windows.zip" } },
            {  "SGminerGM", new List<string>{ "https://github.com/nicehash/sgminer-gm/releases/download/5.5.5-8/sgminer-5.5.5-gm-nicehash-8-windows-amd64.zip" } },
            {  "TeamRedMiner", new List<string>{ "https://github.com/todxx/teamredminer/releases/download/0.5.2/teamredminer-v0.5.2-win.zip" } },
            {  "TRex", new List<string>{ "https://github.com/trexminer/T-Rex/releases/download/0.12.0/t-rex-0.12.0-win-cuda10.0.zip" } },
            {  "TTMiner", new List<string>{ "https://tradeproject.de/download/Miner/TT-Miner-2.2.5.zip" } },
            {  "XmrStak", new List<string>{ "https://github.com/nicehash/xmr-stak/releases/download/nhm-2.10.5/xmr-stak_2.10.5_495625c.7z" } },
            {  "VC_REDIST_x64_2015", new List<string>{ "https://github.com/nicehash/NiceHashMinerTest/releases/download/1.9.1.5/vc_redist.x64.exe.7z" } },
            {  "NanoMiner", new List<string>{ "https://github.com/nanopool/nanominer/releases/download/v1.3.4/nanominer-windows-1.3.4.zip" } },
            {  "WildRig", new List<string>{ "https://github.com/andru-kun/wildrig-multi/releases/download/0.17.9/wildrig-multi-windows-0.17.9-beta.7z" }}
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

        public static IEnumerable<string> GetMinerBinsUrlsForPlugin(string pluginUUID)
        {
            if (_pluginsUrls.ContainsKey(pluginUUID) && _pluginsUrls[pluginUUID] != null && _pluginsUrls[pluginUUID].Count > 0)
            {
                return _pluginsUrls[pluginUUID];
            }

            return Enumerable.Empty<string>();
        }
    }
}
