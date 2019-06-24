#if !ENABLE_EXTERNAL_PLUGINS && (TESTNET || TESTNETDEV) 
#define ENABLE_EXTERNAL_PLUGINS
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginLoader;
using Newtonsoft.Json;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common;
using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using NHM.MinersDownloader;
using NiceHashMiner.Utils;

// alias
using CommonAlgorithm = NiceHashMinerLegacy.Common.Algorithm;
using System.Globalization;

// TODO fix up the namespace
namespace NiceHashMiner.Plugin
{
    public static class MinerPluginsManager
    {
#if ENABLE_EXTERNAL_PLUGINS
        public static bool IntegratedPluginsOnly => false;
#else
        public static bool IntegratedPluginsOnly => true;
#endif

        public static List<IntegratedPlugin> IntegratedPlugins = new List<IntegratedPlugin>
        {
            //// testing 
            //new BrokenPluginIntegratedPlugin(),
            // open source
            new CCMinerMTPIntegratedPlugin(),
            new CCMinerTpruvotIntegratedPlugin(),
            new CCMinerX16RIntegratedPlugin(),
            //new SGminerAvemoreIntegratedPlugin(),
            new SGminerGMIntegratedPlugin(),
            new XmrStakIntegratedPlugin(),

            // 3rd party
            new BMinerIntegratedPlugin(),
            new ClaymoreDualIntegratedPlugin(),
            new EWBFIntegratedPlugin(),
            new GMinerIntegratedPlugin(),
            new NBMinerIntegratedPlugin(),
            new PhoenixIntegratedPlugin(),
            new TeamRedMinerIntegratedPlugin(),
            new TRexIntegratedPlugin(),
            new TTMinerIntegratedPlugin(),
            new NanoMinerIntegratedPlugin(),
            new ClaymoreDual14IntegratedPlugin(),

            // service plugin
            EthlargementIntegratedPlugin.Instance,

            // plugin dependencies
            VC_REDIST_x64_2015_DEPENDENCY_PLUGIN.Instance
        };

        private static HashSet<string> _compatiblePlugins = new HashSet<string>();
        private static Dictionary<string, bool> _integratedPluginsInitialized = new Dictionary<string, bool>();
        private static Dictionary<string, Dictionary<BaseDevice, IReadOnlyList<CommonAlgorithm.Algorithm>>> _integratedPluginsCachedAlgorithms = new Dictionary<string, Dictionary<BaseDevice, IReadOnlyList<CommonAlgorithm.Algorithm>>>();

        // TODO add use3rdParty flag
        public static void InitIntegratedPlugins()
        {
            var is3rdPartyEnabled = ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            // get devices
            var allDevs = AvailableDevices.Devices;
            var baseDevices = allDevs.Select(dev => dev.BaseDevice);
            // examine all plugins and what to use
            foreach (var plugin in IntegratedPlugins)
            {
                //_compatiblePlugins.Add(plugin.PluginUUID); // TODO to download all miners uncomment this line
                var pluginUuid = plugin.PluginUUID;
                var pluginName = plugin.Name;


                if (plugin.Is3rdParty && !is3rdPartyEnabled) continue;
                if (_integratedPluginsInitialized.ContainsKey(pluginUuid) && _integratedPluginsInitialized[pluginUuid])
                {
                    // add from cache
                    var supportedCached = _integratedPluginsCachedAlgorithms[pluginUuid];
                    foreach (var pair in supportedCached)
                    {
                        var bd = pair.Key;
                        var algos = pair.Value;
                        var dev = AvailableDevices.GetDeviceWithUuid(bd.UUID);
                        var pluginAlgos = algos
                        .Where(a => SupportedAlgorithmsFilter.IsSupported(a.IDs))
                        .Select(a => new PluginAlgorithm(pluginName, a, plugin.Version))
                        .ToList();
                        dev.UpdatePluginAlgorithms(pluginUuid, pluginAlgos);
                    }
                    continue;
                }

                // register and add 
                if (plugin is IBackroundService) _compatiblePlugins.Add(pluginUuid);
                if (plugin is IPluginDependency) _compatiblePlugins.Add(pluginUuid);
                var supported = plugin.GetSupportedAlgorithms(baseDevices);
                _integratedPluginsCachedAlgorithms[pluginUuid] = supported;
                // check out the supported algorithms
                foreach (var pair in supported)
                {
                    _compatiblePlugins.Add(pluginUuid);
                    var bd = pair.Key;
                    var algos = pair.Value;
                    var dev = AvailableDevices.GetDeviceWithUuid(bd.UUID);
                    var pluginAlgos = algos
                        .Where(a => SupportedAlgorithmsFilter.IsSupported(a.IDs))
                        .Select(a => new PluginAlgorithm(pluginName, a, plugin.Version))
                        .ToList();
                    dev.UpdatePluginAlgorithms(pluginUuid, pluginAlgos);
                }
            }
            foreach (var plugin in IntegratedPlugins)
            {
                var pluginUuid = plugin.PluginUUID;
                if (plugin.Is3rdParty && !is3rdPartyEnabled) continue;
                if (_compatiblePlugins.Contains(pluginUuid) == false) continue;
                if (_integratedPluginsInitialized.ContainsKey(pluginUuid) && _integratedPluginsInitialized[pluginUuid]) continue;
                if (plugin is IInitInternals pluginWithInternals) pluginWithInternals.InitInternals();
                _integratedPluginsInitialized[pluginUuid] = true;
            }

            CheckPluginsReBenchmarkAlgorithmsForDevices();

            EthlargementIntegratedPlugin.Instance.ServiceEnabled = ConfigManager.GeneralConfig.UseEthlargement && Helpers.IsElevated && is3rdPartyEnabled;

            if (is3rdPartyEnabled) return;
            // filter out 3rdParty
            var thirdPartyPluginUUIDs = IntegratedPlugins
                .Where(plugin => plugin.Is3rdParty)
                .Select(plugin => plugin.PluginUUID);
            foreach (var uuid in thirdPartyPluginUUIDs)
            {
                RemovePluginAlgorithms(uuid);
            }
            Logger.Info("MinerPluginsManager", "Finished initialization of miners.");
        }

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
            // TODO only integrated
            InitIntegratedPlugins();
            if (IntegratedPluginsOnly) return;

            MinerPluginHost.LoadPlugins(Paths.MinerPluginsPath());
            // init internals
            InitPluginInternals();
            UpdatePluginAlgorithms();
            // cross reference local and online list
            CrossReferenceInstalledWithOnline();
        }

        // for now integrated only, it should be safe to call this multiple times 
        public static async Task DevicesCrossReferenceIDsWithMinerIndexes()
        {
            var is3rdPartyEnabled = ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            // get devices
            var allDevs = AvailableDevices.Devices;
            var baseDevices = allDevs.Select(dev => dev.BaseDevice);
            foreach (var plugin in IntegratedPlugins)
            {
                var pluginUuid = plugin.PluginUUID;
                if (plugin.Is3rdParty && !is3rdPartyEnabled) continue;
                if (plugin is IDevicesCrossReference pluginWithDCR)
                {
                    try
                    {
                        await pluginWithDCR.DevicesCrossReference(baseDevices);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("MinerPluginsManager", $"Error occured while executing device cross reference in {plugin.Name} plugin: {e.Message}");
                    }
                }
            }
        }

        public static async Task DownloadMissingIntegratedMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            var compatiblePlugins = IntegratedPlugins
                .Where(p => _compatiblePlugins.Contains(p.PluginUUID))
                .Where(p => p is IMinerBinsSource)
                .Where(p => p is IBinaryPackageMissingFilesChecker)
                .ToArray();

            foreach (var plugin in compatiblePlugins)
            {
                var downloadSource = plugin as IMinerBinsSource;
                var isMissingCheck = plugin as IBinaryPackageMissingFilesChecker;
                var urls = downloadSource.GetMinerBinsUrls();
                var missingFiles = isMissingCheck.CheckBinaryPackageMissingFiles();
                var hasMissingFiles = missingFiles.Count() > 0;
                var hasUrls = urls.Count() > 0;
                if (hasMissingFiles && hasUrls)
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
            var ret = new List<string>();
            foreach (var plugin in IntegratedPlugins)
            {
                if (_compatiblePlugins.Contains(plugin.PluginUUID) == false) continue;
                if (plugin is IBinaryPackageMissingFilesChecker pluginCheckBins)
                {
                    try
                    {
                        ret.AddRange(pluginCheckBins.CheckBinaryPackageMissingFiles());
                    }
                    catch (Exception e)
                    {
                        Logger.Error("MinerPluginsManager", $"Error occured while checking for missing miners: {e.Message}");
                    }
                }
            }
            return ret;
        }

        private static void UpdatePluginAlgorithms()
        {
            // get devices
            var allDevs = AvailableDevices.Devices;
            var baseDevices = allDevs.Select(dev => dev.BaseDevice);
            // examine all plugins and what to use
            foreach (var kvp in MinerPluginHost.MinerPlugin)
            {
                var pluginUuid = kvp.Key;
                var plugin = kvp.Value;
                var pluginName = plugin.Name;
                var supported = plugin.GetSupportedAlgorithms(baseDevices);
                // check out the supported algorithms
                foreach (var pair in supported)
                {
                    var bd = pair.Key;
                    var algos = pair.Value;
                    var dev = AvailableDevices.GetDeviceWithUuid(bd.UUID);
                    var pluginAlgos = algos
                        .Where(a => SupportedAlgorithmsFilter.IsSupported(a.IDs))
                        .Select(a => new PluginAlgorithm(pluginName, a, plugin.Version))
                        .ToList();
                    dev.UpdatePluginAlgorithms(pluginUuid, pluginAlgos);
                }
            }
            CheckPluginsReBenchmarkAlgorithmsForDevices();
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
                Logger.Error("MinerPluginsManager", $"Error occured while removing {pluginUUID} plugin: {e.Message}");
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

        public static List<string> GetPluginUUIDsAndVersionsList()
        {
            var ret = new List<string>();
            foreach (var integrated in IntegratedPlugins)
            {
                ret.Add($"{integrated.PluginUUID}-{integrated.Version.Major}.{integrated.Version.Minor}");
            }
            if (IntegratedPluginsOnly) return ret;

            foreach (var kvp in Plugins)
            {
                var plugin = kvp.Value;
                if (plugin.Installed)
                {
                    ret.Add($"{plugin.PluginUUID}-{plugin.PluginVersion.Major}.{plugin.PluginVersion.Minor}");
                }
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
            //const string pluginsJsonApiUrl = "https://miner-plugins.nicehash.com/api/plugins";
            const string pluginsJsonApiUrl = "https://miner-plugins-test-dev.nicehash.com/api/plugins";
            try
            {
                using (var client = new NoKeepAlivesWebClient())
                {
                    string s = client.DownloadString(pluginsJsonApiUrl);
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

        private static void CheckPluginsReBenchmarkAlgorithmsForDevices()
        {
            // Integrated
            var reBenchmarkCheckersPlugins = IntegratedPlugins.Where(plugin => plugin is IReBenchmarkChecker).Cast<IMinerPlugin>().ToList();
            if (!IntegratedPluginsOnly)
            {
                // Dynamic/Online
                var dynamicReBenchmarkCheckers = MinerPluginHost.MinerPlugin.Values.Where(plugin => plugin is IReBenchmarkChecker).ToList();
                if (dynamicReBenchmarkCheckers.Count > 0) reBenchmarkCheckersPlugins.AddRange(dynamicReBenchmarkCheckers);
            }

            // get devices
            var allDevs = AvailableDevices.Devices;
            foreach (var plugin in reBenchmarkCheckersPlugins)
            {
                try
                {
                    var reBenchCheckPlugin = plugin as IReBenchmarkChecker;
                    if (reBenchCheckPlugin == null) continue;
                    foreach (var dev in allDevs)
                    {
                        var baseDev = dev.BaseDevice;
                        var pluginAlgos = dev.AlgorithmSettings.Where(a => a.MinerUUID == plugin.PluginUUID).ToArray();
                        foreach (var algo in pluginAlgos)
                        {
                            var pAlgo = algo as PluginAlgorithm;
                            if (pAlgo == null) continue;
                            var isReBenchmark = reBenchCheckPlugin.ShouldReBenchmarkAlgorithmOnDevice(baseDev, pAlgo.ConfigVersion, pAlgo.IDs);
                            pAlgo.IsReBenchmark = isReBenchmark;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("MinerPluginsManager", $"CheckPluginsReBenchmarkAlgorithmsForDevices error: {e.Message}");
                }
            }
        }

        public static IMinerPlugin GetPluginWithUuid(string pluginUuid)
        {
            // search for integrated
            var integratedPlugin = IntegratedPlugins.Find(p => p.PluginUUID == pluginUuid);
            if (integratedPlugin != null) return integratedPlugin;
            if (!MinerPluginHost.MinerPlugin.ContainsKey(pluginUuid)) return null;
            return MinerPluginHost.MinerPlugin[pluginUuid];
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
                Logger.Error("MinerPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: ${e.Message}");
                //downloadAndInstallUpdate();
            }
        }
        #endregion DownloadingInstalling
    }
}
