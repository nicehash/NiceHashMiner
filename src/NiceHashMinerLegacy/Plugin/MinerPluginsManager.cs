using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1.Interfaces;
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

        public static List<PluginPackageInfoCR> RankedPlugins
        {
            get
            {
                var orderedByDeviceSupportCountAndName = Plugins
                    .Select(kvp => kvp.Value)
                    .OrderByDescending(info => info.HasNewerVersion)
                    .OrderByDescending(info => info.OnlineSupportedDeviceCount)
                    .ThenBy(info => info.PluginName);
                return orderedByDeviceSupportCountAndName.ToList();
            }
        }

        private static void InitPluginInternals()
        {
            foreach (var kvp in MinerPlugins)
            {
                var plugin = kvp.Value;
                if (plugin is IInitInternals pluginWithInternals) pluginWithInternals.InitInternals();
            }
        }

        public static void LoadMinerPlugins()
        {
            MinerPluginHost.LoadPlugins(Paths.MinerPluginsPath());
            // init internals
            InitPluginInternals();
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

                Plugins[pluginUUID].LocalInfo = null;
                // TODO we might not have any online reference so remove it in this case
                if (Plugins[pluginUUID].OnlineInfo == null)
                {
                    Plugins.Remove(pluginUUID);
                }

                CrossReferenceInstalledWithOnline();
                // TODO before deleting you will need to unload the dll
                if (Directory.Exists(deletePath))
                {
                    Directory.Delete(deletePath, true);
                }
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
                var localPluginInfo = new PluginPackageInfo
                {
                    PluginAuthor = installed.Author,
                    PluginName = installed.Name,
                    PluginUUID = uuid,
                    PluginVersion = installed.Version,
                    // other stuff is not inside the plugin
                };
                if (Plugins.ContainsKey(uuid) == false)
                {
                    Plugins[uuid] = new PluginPackageInfoCR{};
                }
                Plugins[uuid].LocalInfo = localPluginInfo;
            }

            // get online list and check what we have and what is online
            if (GetOnlineMinerPlugins() == false || OnlinePlugins == null) return;

            foreach (var online in OnlinePlugins)
            {
                var uuid = online.PluginUUID;
                if (Plugins.ContainsKey(uuid) == false)
                {
                    Plugins[uuid] = new PluginPackageInfoCR{};
                }
                Plugins[uuid].OnlineInfo = online;
                if (online.SupportedDevicesAlgorithms != null)
                {
                    var supportedDevices = online.SupportedDevicesAlgorithms
                        .Where(kvp => kvp.Value.Count > 0)
                        .Select(kvp => kvp.Key);
                    var devRank = AvailableDevices.Devices
                        .Where(d => supportedDevices.Contains(d.DeviceType.ToString()))
                        .Count();
                    Plugins[uuid].OnlineSupportedDeviceCount = devRank;
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
                    string s = client.DownloadString(pluginsJsonApiUrl);
                    //// local fake string
                    //string s = Properties.Resources.pluginJSON;
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

        #region Downloading

        // TODO refactor ProgressState this tells us in what kind of a state the installation is
        public enum ProgressState
        {
            Started,
            DownloadingPlugin,
            ExtractingPlugin,
            DownloadingMiner,
            ExtractingMiner,
            // TODO loading
        }

        public delegate void DownloadAndInstallUpdate(ProgressState state, int progress);
        //public delegate void DownloadAndInstallUpdate(string infoStr);

        public static async Task DownloadAndInstall(PluginPackageInfoCR plugin, DownloadAndInstallUpdate downloadAndInstallUpdate, CancellationToken stop)
        {
            var downloadPluginProgressChangedEventHandler = new DownloadProgressChangedEventHandler((s, e1) => {
                downloadAndInstallUpdate?.Invoke(ProgressState.DownloadingPlugin, e1.ProgressPercentage);
            });
            OnZipProgres zipProgressPluginChangedEventHandler = (int progress) => {
                downloadAndInstallUpdate?.Invoke(ProgressState.ExtractingPlugin, progress);
            };
            var downloadMinerProgressChangedEventHandler = new DownloadProgressChangedEventHandler((s, e1) => {
                downloadAndInstallUpdate?.Invoke(ProgressState.DownloadingMiner, e1.ProgressPercentage);
            });
            OnZipProgres zipProgressMinerChangedEventHandler = (int progress) => {
                downloadAndInstallUpdate?.Invoke(ProgressState.ExtractingMiner, progress);
            };



            const string installingPrefix = "installing_";
            var installingPluginPath = Path.Combine(Paths.MinerPluginsPath(), $"{installingPrefix}{plugin.PluginUUID}");
            try
            {
                //downloadAndInstallUpdate("Starting");
                Directory.CreateDirectory(installingPluginPath);

                // download plugin dll
                var pluginPackageDownloaded = Path.Combine(installingPluginPath, "plugin.zip");
                var downloadPluginOK = await DownloadFile(plugin.PluginPackageURL, pluginPackageDownloaded, downloadPluginProgressChangedEventHandler, stop);
                if (!downloadPluginOK || stop.IsCancellationRequested) return;
                // unzip 
                var unzipPluginOK = await UnzipFile(pluginPackageDownloaded, installingPluginPath, zipProgressPluginChangedEventHandler, stop);
                if (!unzipPluginOK || stop.IsCancellationRequested) return;
                File.Delete(pluginPackageDownloaded);

                // download plugin dll
                var binsPackageDownloaded = Path.Combine(installingPluginPath, "miner_bins.zip");
                var downloadMinerBinsOK = await DownloadFile(plugin.MinerPackageURL, binsPackageDownloaded, downloadMinerProgressChangedEventHandler, stop);
                if (!downloadMinerBinsOK || stop.IsCancellationRequested) return;
                // unzip 
                var binsUnzipPath = Path.Combine(installingPluginPath, "bins");
                var unzipMinerBinsOK = await UnzipFile(binsPackageDownloaded, binsUnzipPath, zipProgressMinerChangedEventHandler, stop);
                if (!unzipMinerBinsOK || stop.IsCancellationRequested) return;
                File.Delete(binsPackageDownloaded);


                var loadedPlugins = MinerPluginHost.LoadPlugin(installingPluginPath);
                if (loadedPlugins == 0)
                {
                    //downloadAndInstallUpdate($"Loaded ZERO PLUGINS");
                    Directory.Delete(installingPluginPath, true);
                    return;
                }

                //downloadAndInstallUpdate("Checking old plugin");
                var pluginPath = Path.Combine(Paths.MinerPluginsPath(), plugin.PluginUUID);
                // if there is an old plugin installed remove it
                if (Directory.Exists(pluginPath))
                {
                    Directory.Delete(pluginPath, true);
                }
                //downloadAndInstallUpdate($"Loaded {loadedPlugins} PLUGIN");
                Directory.Move(installingPluginPath, pluginPath);
                UpdatePluginAlgorithms();
                // cross reference local and online list
                CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("MinerPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: ${e}");
                //downloadAndInstallUpdate();
            }
        }

        public static async Task<bool> DownloadFile(string url, string downloadFileLocation, DownloadProgressChangedEventHandler downloadProgressChangedEventHandler, CancellationToken stop)
        {
            var downloadStatus = false;
            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += downloadProgressChangedEventHandler;
                client.DownloadFileCompleted += (s, e) =>
                {
                    downloadStatus = !e.Cancelled && e.Error == null;
                };
                stop.Register(client.CancelAsync);
                // Starts the download
                await client.DownloadFileTaskAsync(new Uri(url), downloadFileLocation);
            }
            return downloadStatus;
        }

        public delegate void OnZipProgres(int prog);

        public static async Task<bool> UnzipFile(string zipLocation, string unzipLocation, OnZipProgres zipProgressChangedEventHandler, CancellationToken stop)
        {
            return await Task.Run(() => {
                using (var archive = ZipFile.OpenRead(zipLocation))
                {
                    float entriesCount = archive.Entries.Count;
                    float extractedEntries = 0;
                    foreach (var entry in archive.Entries)
                    {
                        if (stop.IsCancellationRequested) break;

                        extractedEntries += 1;
                        var isDirectory = entry.Name == "";
                        if (isDirectory) continue;

                        var prog = ((extractedEntries / entriesCount) * 100.0f);
                        zipProgressChangedEventHandler((int)prog);

                        var extractPath = Path.Combine(unzipLocation, entry.FullName);
                        var dirPath = Path.GetDirectoryName(extractPath);
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                        }
                        entry.ExtractToFile(extractPath, true);
                    }
                }
                return true;
            }, stop);
        }
        #endregion Downloading
    }
}
