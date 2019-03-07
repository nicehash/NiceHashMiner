using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPlugin.Toolkit;
using MinerPluginLoader;
using Newtonsoft.Json;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;

// TODO fix up the namespace
namespace NiceHashMiner.Plugin
{
    public static class MinerPluginsManager
    {
        const string PluginsPath = "miner_plugins";

        public static List<PluginPackageInfo> OnlinePlugins { get; private set; }
        public static Dictionary<string, IMinerPlugin> MinerPlugin { get => MinerPluginHost.MinerPlugin; }

        public static void LoadMinerPlugins()
        {
            GetOnlineMinerPlugins();
            MinerPluginHost.LoadPlugins(PluginsPath, SearchOption.AllDirectories);
            // get devices
            var allDevs = AvailableDevices.Devices;
            var baseDevices = allDevs.Select(dev => dev.PluginDevice);
            // examine all plugins and what to use
            foreach (var kvp in MinerPluginHost.MinerPlugin)
            {
                var pluginUuid = kvp.Key;
                var plugin = kvp.Value;
                var supported = plugin.GetSupportedAlgorithms(baseDevices);
                // check out the supported algorithms
                foreach (var pair in supported)
                {
                    var bd = pair.Key;
                    var algos = pair.Value;
                    var dev = AvailableDevices.GetDeviceWithUuid(bd.UUID);
                    var pluginAlgos = algos.Select(a => new PluginAlgorithm(a)).ToList();
                    dev.UpdatePluginAlgorithms(pluginUuid, pluginAlgos);
                }
            }
        }

        public static bool GetOnlineMinerPlugins()
        {
            const string pluginsJsonApiUrl = "https://miner-plugins.nicehash.com/plugins.json";
            try
            {
                using (var client = new WebClient())
                {
                    string s = client.DownloadString(pluginsJsonApiUrl);
                    var onlinePlugins = JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, Globals.JsonSettings);
                    OnlinePlugins = onlinePlugins;
                }

                return true;
            } catch(Exception e)
            {
                Helpers.ConsolePrint("MinerPluginsManager", $"GetOnlineMinerPlugins ex: {e}");
            }
            return false;
        }

        public static IMinerPlugin GetPluginWithUuid(string pluginUuid)
        {
            if (!MinerPluginHost.MinerPlugin.ContainsKey(pluginUuid)) return null;
            return MinerPluginHost.MinerPlugin[pluginUuid];
        }
    }
}
