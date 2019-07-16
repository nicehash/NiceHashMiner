#if !ENABLE_EXTERNAL_PLUGINS && (TESTNET || TESTNETDEV || PRODUCTION_NEW) 
#define ENABLE_EXTERNAL_PLUGINS
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MinerPluginLoader;
using Newtonsoft.Json;
using NiceHashMiner.Devices;
using NHM.Common;
using NiceHashMiner.Configs;
using NHM.Common.Enums;
using NHM.MinersDownloader;
using NiceHashMiner.Utils;
using System.Globalization;

namespace NiceHashMiner.Mining.Plugins
{
    public static class MinerPluginsManager
    {
#if ENABLE_EXTERNAL_PLUGINS
        public static bool IntegratedPluginsOnly => false;
#else
        public static bool IntegratedPluginsOnly => true;
#endif

        static MinerPluginsManager()
        {
            var integratedPlugins = new List<IntegratedPlugin>
            {
                ////// testing 
                //new BrokenPluginIntegratedPlugin(),
                // open source
                new CCMinerMTPIntegratedPlugin(),
                new CCMinerTpruvotIntegratedPlugin(),
                new SGminerAvemoreIntegratedPlugin(),
                new SGminerGMIntegratedPlugin(),
                new XmrStakIntegratedPlugin(),

                // 3rd party
                new BMinerIntegratedPlugin(),
                new EWBFIntegratedPlugin(),
                new GMinerIntegratedPlugin(),
                new NBMinerIntegratedPlugin(),
                new PhoenixIntegratedPlugin(),
                new TeamRedMinerIntegratedPlugin(),
                new TRexIntegratedPlugin(),
                new TTMinerIntegratedPlugin(),
                new ClaymoreDual14IntegratedPlugin(),

                // can be integrated but are not included
                // new NanoMinerIntegratedPlugin(),
                // new WildRigIntegratedPlugin(),
                // new CryptoDredgeIntegratedPlugin(),

                // service plugin
                EthlargementIntegratedPlugin.Instance,

                // plugin dependencies
                VC_REDIST_x64_2015_DEPENDENCY_PLUGIN.Instance
            };
            var filteredIntegratedPlugins = integratedPlugins.Where(p => SupportedPluginsFilter.IsSupported(p.PluginUUID)).ToList();
            foreach (var integratedPlugin in filteredIntegratedPlugins)
            {
                PluginContainer.Create(integratedPlugin);
            }
        }

        public static void InitIntegratedPlugins()
        {
            foreach (var plugin in PluginContainer.PluginContainers.Where(p => p.IsIntegrated))
            {
                if (!plugin.IsInitialized)
                {
                    plugin.InitPluginContainer();
                }
                if (plugin.Enabled)
                {
                    plugin.AddAlgorithmsToDevices();
                }
                else
                {
                    plugin.RemoveAlgorithmsFromDevices();
                }
            }

            // global scope here
            var is3rdPartyEnabled = ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            EthlargementIntegratedPlugin.Instance.ServiceEnabled = ConfigManager.GeneralConfig.UseEthlargement && Helpers.IsElevated && is3rdPartyEnabled;
            Logger.Info("MinerPluginsManager", "Finished initialization of miners.");
        }

        // API data
        private static List<PluginPackageInfo> OnlinePlugins { get; set; }
        public static Dictionary<string, PluginPackageInfoCR> Plugins { get; set; } = new Dictionary<string, PluginPackageInfoCR>();

        //private static Dictionary<string, IMinerPlugin> MinerPlugins { get => MinerPluginHost.MinerPlugin; }

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

        public static void LoadMinerPlugins()
        {
            // TODO only integrated
            InitIntegratedPlugins();
            if (IntegratedPluginsOnly) return;
            var loadedPlugins = MinerPluginHost.LoadPlugins(Paths.MinerPluginsPath());
            foreach (var pluginUUID in loadedPlugins)
            {
                var externalPlugin = MinerPluginHost.MinerPlugin[pluginUUID];
                var plugin = PluginContainer.Create(externalPlugin);
                if (!plugin.IsInitialized)
                {
                    plugin.InitPluginContainer();
                }
                if (plugin.Enabled)
                {
                    plugin.AddAlgorithmsToDevices();
                }
                else
                {
                    plugin.RemoveAlgorithmsFromDevices();
                }
            }
            // cross reference local and online list
            CrossReferenceInstalledWithOnline();
        }

        public static async Task DevicesCrossReferenceIDsWithMinerIndexes()
        {
            // get devices
            var baseDevices = AvailableDevices.Devices.Select(dev => dev.BaseDevice);
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatible)
                .Where(p => p.Enabled)
                .ToArray();
            foreach (var plugin in checkPlugins)
            {
                await plugin.DevicesCrossReference(baseDevices);
            }
        }

        public static async Task DownloadMissingIntegratedMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsIntegrated)
                .Where(p => p.IsCompatible)
                .Where(p => p.Enabled)
                .ToArray();

            foreach (var plugin in checkPlugins)
            {
                var urls = plugin.GetMinerBinsUrls();
                var missingFiles = plugin.CheckBinaryPackageMissingFiles();
                var hasMissingFiles = missingFiles.Count() > 0;
                var hasUrls = urls.Count() > 0;
                if (hasMissingFiles && hasUrls && !plugin.IsBroken)
                {
                    var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Downloading {0} %", $"{plugin.Name} {perc}"), perc)));
                    var unzipProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Unzipping {0} %", $"{plugin.Name} {perc}"), perc)));
                    await DownloadInternalBins(plugin.PluginUUID, urls.ToList(), downloadProgress, unzipProgress, stop);
                }
            }
        }

        // for now integrated only
        public static List<string> GetMissingMiners()
        {
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatible)
                .Where(p => p.Enabled)
                .ToArray();

            var ret = new List<string>();
            foreach (var plugin in checkPlugins)
            {
                ret.AddRange(plugin.CheckBinaryPackageMissingFiles());
            }
            return ret;
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
                var oldPlugins = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUUID).ToArray();
                foreach (var old in oldPlugins)
                {
                    PluginContainer.RemovePluginContainer(old);
                    old.RemoveAlgorithmsFromDevices();
                }
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
                Logger.Error("MinerPluginsManager", $"Error occured while removing {pluginUUID} plugin: {e.Message}");
            }       
        }

        public static void CrossReferenceInstalledWithOnline()
        {
            // first go over the installed plugins
            // TODO rename installed to externalInstalledPlugin
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => !p.IsIntegrated)
                //.Where(p => p.IsCompatible)
                //.Where(p => p.Enabled)
                .ToArray();
            foreach (var installed in checkPlugins)
            {
                var uuid = installed.PluginUUID;
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

        public static List<string> GetPluginUUIDsAndVersionsList()
        {
            var ret = new List<string>();
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatible)
                .Where(p => p.Enabled)
                .ToArray();
            foreach (var integrated in checkPlugins)
            {
                ret.Add($"{integrated.PluginUUID}-{integrated.Version.Major}.{integrated.Version.Minor}");
            }
            return ret;
        }


        private class NoKeepAlivesWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    ((HttpWebRequest)request).KeepAlive = false;
                }

                return request;
            }
        }

        // TODO this here is blocking
        public static bool GetOnlineMinerPlugins()
        {
            try
            {
                using (var client = new NoKeepAlivesWebClient())
                {
                    string s = client.DownloadString(Links.PluginsJsonApiUrl);
                    //// local fake string
                    //string s = Properties.Resources.pluginJSON;
                    var onlinePlugins = JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Culture = CultureInfo.InvariantCulture
                    });
                    OnlinePlugins = onlinePlugins;
                }

                return true;
            } catch(Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Error occured while getting online miner plugins: {e.Message}");
            }
            return false;
        }

        public static PluginContainer GetPluginWithUuid(string pluginUuid)
        {
            var ret = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUuid).FirstOrDefault();
            return ret;
        }

#region DownloadingInstalling

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

        public static async Task DownloadInternalBins(string pluginUUID, List<string> urls, IProgress<int> downloadProgress, IProgress<int> unzipProgress, CancellationToken stop)
        {
            var installingPluginBinsPath = Path.Combine(Paths.MinerPluginsPath(), pluginUUID, "bins");
            try
            {
                if (Directory.Exists(installingPluginBinsPath)) Directory.Delete(installingPluginBinsPath, true);
                //downloadAndInstallUpdate("Starting");
                Directory.CreateDirectory(installingPluginBinsPath);
                var installedBins = false;
                foreach (var url in urls)
                {
                    // download plugin dll
                    var downloadMinerBinsResult = await MinersDownloadManager.DownloadFileAsync(url, installingPluginBinsPath, "miner_bins", downloadProgress, stop);
                    var binsPackageDownloaded = downloadMinerBinsResult.downloadedFilePath;
                    var downloadMinerBinsOK = downloadMinerBinsResult.success;
                    if (!downloadMinerBinsOK || stop.IsCancellationRequested) return;
                    // unzip 
                    var binsUnzipPath = installingPluginBinsPath; // Path.Combine(installingPluginPath, "bins");
                    var unzipMinerBinsOK = await ArchiveHelpers.ExtractFileAsync(binsPackageDownloaded, binsUnzipPath, unzipProgress, stop);
                    if (stop.IsCancellationRequested) return;
                    if (unzipMinerBinsOK)
                    {
                        installedBins = true;
                        File.Delete(binsPackageDownloaded);
                        break;
                    }   
                }
                if (!installedBins)
                {
                    Logger.Error("MinerPluginsManager", $"Miners bins of {pluginUUID} not installed");
                }
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Installation of {pluginUUID} failed: ${e.Message}");
            }
        }

        public static async Task DownloadAndInstall(PluginPackageInfoCR plugin, IProgress<Tuple<ProgressState, int>> progress, CancellationToken stop)
        {
            var downloadPluginProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(ProgressState.DownloadingPlugin, perc)));
            var zipProgressPluginChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(ProgressState.ExtractingPlugin, perc)));
            var downloadMinerProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(ProgressState.DownloadingMiner, perc)));
            var zipProgressMinerChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(ProgressState.ExtractingMiner, perc)));

            const string installingPrefix = "installing_";
            var installingPluginPath = Path.Combine(Paths.MinerPluginsPath(), $"{installingPrefix}{plugin.PluginUUID}");
            try
            {
                if (Directory.Exists(installingPluginPath)) Directory.Delete(installingPluginPath, true);
                //downloadAndInstallUpdate("Starting");
                Directory.CreateDirectory(installingPluginPath);

                // download plugin dll
                var downloadPluginResult = await MinersDownloadManager.DownloadFileAsync(plugin.PluginPackageURL, installingPluginPath, "plugin", downloadPluginProgressChangedEventHandler, stop);
                var pluginPackageDownloaded = downloadPluginResult.downloadedFilePath;
                var downloadPluginOK = downloadPluginResult.success;
                if (!downloadPluginOK || stop.IsCancellationRequested) return;
                // unzip 
                var unzipPluginOK = await ArchiveHelpers.ExtractFileAsync(pluginPackageDownloaded, installingPluginPath, zipProgressPluginChangedEventHandler, stop);
                if (!unzipPluginOK || stop.IsCancellationRequested) return;
                File.Delete(pluginPackageDownloaded);

                // download plugin dll
                var downloadMinerBinsResult = await MinersDownloadManager.DownloadFileAsync(plugin.MinerPackageURL, installingPluginPath, "miner_bins", downloadMinerProgressChangedEventHandler, stop);
                var binsPackageDownloaded = downloadMinerBinsResult.downloadedFilePath;
                var downloadMinerBinsOK = downloadMinerBinsResult.success;
                if (!downloadMinerBinsOK || stop.IsCancellationRequested) return;
                // unzip 
                var binsUnzipPath = Path.Combine(installingPluginPath, "bins");
                var unzipMinerBinsOK = await ArchiveHelpers.ExtractFileAsync(binsPackageDownloaded, binsUnzipPath, zipProgressMinerChangedEventHandler, stop);
                if (!unzipMinerBinsOK || stop.IsCancellationRequested) return;
                File.Delete(binsPackageDownloaded);


                var loadedPlugins = MinerPluginHost.LoadPlugin(installingPluginPath);
                if (loadedPlugins.Count() == 0)
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
                // add or update plugins
                foreach (var pluginUUID in loadedPlugins)
                {
                    var externalPlugin = MinerPluginHost.MinerPlugin[pluginUUID];
                    // remove old
                    var oldPlugins = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUUID).ToArray();
                    foreach (var old in oldPlugins)
                    {
                        PluginContainer.RemovePluginContainer(old);
                        old.RemoveAlgorithmsFromDevices();
                    }
                    var newPlugin = PluginContainer.Create(externalPlugin);
                    var success = newPlugin.InitPluginContainer();
                    // TODO after add or remove plugins we should clean up the device settings
                    if (success)
                    {
                        newPlugin.AddAlgorithmsToDevices();
                        await newPlugin.DevicesCrossReference(AvailableDevices.Devices.Select(d => d.BaseDevice));
                    }
                    else
                    {
                        // TODO mark that this plugin wasn't loaded
                        Logger.Error("MinerPluginsManager", $"DownloadAndInstall unable to init and install {pluginUUID}");
                    }
                }
                // cross reference local and online list
                CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: ${e.Message}");
                //downloadAndInstallUpdate();
            }
        }
        #endregion DownloadingInstalling
    }
}
