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
using NiceHashMinerLegacy.Common;

// TODO fix up the namespace
namespace NiceHashMiner.Plugin
{
    public static class MinerPluginsManager
    {
        private static List<PluginPackageInfo> OnlinePlugins { get; set; }
        private static Dictionary<string, IMinerPlugin> MinerPlugins { get => MinerPluginHost.MinerPlugin; }

        public static Dictionary<string, PluginPackageInfoCR> Plugins { get; set; } = new Dictionary<string, PluginPackageInfoCR>();

        public static void LoadMinerPlugins()
        {
            MinerPluginHost.LoadPlugins(Paths.MinerPluginsPath(), SearchOption.AllDirectories);
            UpdatePluginAlgorithms();
            // cross reference local and online list
            CrossReferenceInstalledWithOnline();
        }

        private static void UpdatePluginAlgorithms()
        {
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

        private static void RemovePluginAlgorithms(string pluginUUID)
        {
            foreach (var dev in AvailableDevices.Devices)
            {
                dev.RemovePluginAlgorithms(pluginUUID);
            }
        }

        public static void Remove(string pluginUUID)
        {
            try
            {
                var deletePath = Path.Combine(Paths.MinerPluginsPath(), pluginUUID);
                MinerPluginHost.MinerPlugin.Remove(pluginUUID);
                RemovePluginAlgorithms(pluginUUID);
                CrossReferenceInstalledWithOnline();
                //// TODO before deleting you will need to unload the dll
                //if (Directory.Exists(deletePath))
                //{
                //    Directory.Delete(deletePath, true);
                //    //LoadMinerPlugins();
                //}
            } catch(Exception e)
            {

            }
        }

        public static void CrossReferenceInstalledWithOnline()
        {
            // first go over the installed plugins
            foreach (var installedPluginKvp in MinerPlugins)
            {
                var uuid = installedPluginKvp.Key;
                var installed = installedPluginKvp.Value;

                if (Plugins.ContainsKey(uuid) == false)
                {
                    Plugins[uuid] = new PluginPackageInfoCR
                    {
                        Installed = true,
                        PluginAuthor = installed.Author,
                        PluginName = installed.Name,
                        PluginUUID = uuid,
                        PluginVersion = installed.Version,
                        // other stuff is not inside the plugin
                    };
                }
                else
                {
                    var plugin = Plugins[uuid];
                    // update stuff that might change, after update, take care of uninstall case
                    plugin.Installed = true;
                    plugin.PluginAuthor = installed.Author;
                    plugin.PluginName = installed.Name;
                    plugin.PluginUUID = uuid;
                    plugin.PluginVersion = installed.Version;
                }
            }

            // get online list and check what we have and what is online
            if (GetOnlineMinerPlugins() == false || OnlinePlugins == null) return;

            foreach (var online in OnlinePlugins)
            {
                var uuid = online.PluginUUID;
                if (Plugins.ContainsKey(uuid) == false)
                {
                    Plugins[uuid] = new PluginPackageInfoCR
                    {
                        Installed = false,
                        OnlineVersion = online,
                        PluginUUID = online.PluginUUID,
                        PluginName = online.PluginName,
                        PluginVersion = online.PluginVersion,
                        PluginPackageURL = online.PluginPackageURL,
                        MinerPackageURL = online.MinerPackageURL,
                        SupportedDevicesAlgorithms = online.SupportedDevicesAlgorithms,
                        PluginAuthor = online.PluginAuthor,
                        PluginDescription = online.PluginDescription,
                    };
                }
                else
                {
                    Plugins[uuid].OnlineVersion = online;
                }
            }
            // finally check uninstalled version
            var installedPlugins = Plugins.Where(p => p.Value.Installed);
            foreach (var pluginInfo in installedPlugins)
            {
                if(MinerPlugins.ContainsKey(pluginInfo.Key) == false)
                {
                    // not installed anymore
                    pluginInfo.Value.Installed = false;
                    // TODO we might not have any online reference so remove it in this case
                    if (pluginInfo.Value.OnlineVersion == null)
                    {
                        Plugins.Remove(pluginInfo.Key);
                    }
                }
            }
        }

        public static bool GetOnlineMinerPlugins()
        {
            const string pluginsJsonApiUrl = "https://miner-plugins.nicehash.com/api/plugins";
            try
            {
                using (var client = new WebClient())
                {
                    //string s = client.DownloadString(pluginsJsonApiUrl);
                    // local fake string
                    string s = Properties.Resources.pluginJSON;
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
