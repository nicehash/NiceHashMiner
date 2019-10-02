using MinerPlugin;
using MinerPluginLoader;
using MinerPluginToolkitV1;
using Newtonsoft.Json;
using NHM.Common;
using NHM.MinersDownloader;
using NHMCore.Configs;
using NHMCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Mining.Plugins
{
    public static class MinerPluginsManager
    {
        private static readonly List<IMinerPlugin> _integratedPlugins;
        static MinerPluginsManager()
        {
            // This is just a list of miners that are intergated in the nhm client. usefull when debuging
            _integratedPlugins = new List<IMinerPlugin>
            {
                // testing 
#if INTEGRATE_BrokenMiner_PLUGIN
                new BrokenMiner.BrokenMinerPlugin(),
#endif
#if INTEGRATE_ExamplePlugin_PLUGIN
                new Example.ExamplePlugin(),
#endif

// open source
//#if INTEGRATE_CCMinerMTP_PLUGIN
//                new CCMinerMTP.CCMinerMTPPlugin(), // not compatible with new platform
//#endif
#if INTEGRATE_CCMinerTpruvot_PLUGIN
                new CCMinerTpruvot.CCMinerTpruvotPlugin(),
#endif
#if INTEGRATE_SGminerAvemore_PLUGIN
                new SgminerAvemore.SgminerAvemorePlugin(),
#endif
#if INTEGRATE_SGminerGM_PLUGIN
                new SgminerGM.SgminerGMPlugin(),
#endif
#if INTEGRATE_XmrStak_PLUGIN
                new XmrStak.XmrStakPlugin(),
#endif
#if INTEGRATE_CpuMinerOpt_PLUGIN
                new CpuMinerOpt.CPUMinerPlugin(),
#endif
//#if INTEGRATE_Ethminer_PLUGIN
//                new Ethminer.EthminerPlugin(), // abstract UUID
//#endif

// 3rd party
#if INTEGRATE_EWBF_PLUGIN
                new EWBF.EwbfPlugin(),
#endif
#if INTEGRATE_GMiner_PLUGIN
                new GMinerPlugin.GMinerPlugin(),
#endif
#if INTEGRATE_NBMiner_PLUGIN
                new NBMiner.NBMinerPlugin(),
#endif
#if INTEGRATE_Phoenix_PLUGIN
                new Phoenix.PhoenixPlugin(),
#endif
#if INTEGRATE_TeamRedMiner_PLUGIN
                new TeamRedMiner.TeamRedMinerPlugin(),
#endif
#if INTEGRATE_TRex_PLUGIN
                new TRex.TRexPlugin(),
#endif
#if INTEGRATE_TTMiner_PLUGIN
                new TTMiner.TTMinerPlugin(),
#endif
#if INTEGRATE_ClaymoreDual_PLUGIN
                new ClaymoreDual14.ClaymoreDual14Plugin(),
#endif
#if INTEGRATE_NanoMiner_PLUGIN
                new NanoMiner.NanoMinerPlugin(),
#endif
#if INTEGRATE_WildRig_PLUGIN
                new WildRig.WildRigPlugin(),
#endif
#if INTEGRATE_CryptoDredge_PLUGIN
                new CryptoDredge.CryptoDredgePlugin(),
#endif
#if INTEGRATE_BMiner_PLUGIN
                new BMiner.BMinerPlugin(),
#endif
#if INTEGRATE_ZEnemy_PLUGIN
                new ZEnemy.ZEnemyPlugin(),
#endif
#if INTEGRATE_LolMinerBeam_PLUGIN
                new LolMinerBeam.LolMinerBeamPlugin(),
#endif
//#if INTEGRATE_SRBMiner_PLUGIN
//                new SRBMiner.SRBMinerPlugin(),
//#endif
#if INTEGRATE_XMRig_PLUGIN
                new XMRig.XMRigPlugin(),
#endif
#if INTEGRATE_MiniZ_PLUGIN
                new MiniZ.MiniZPlugin(),
#endif

                // leave these 2 for now

                // service plugin
                EthlargementIntegratedPlugin.Instance,

                // plugin dependencies
                VC_REDIST_x64_2015_2019_DEPENDENCY_PLUGIN.Instance
            };
            var filteredIntegratedPlugins = _integratedPlugins.Where(p => SupportedPluginsFilter.IsSupported(p.PluginUUID)).ToList();
            foreach (var integratedPlugin in filteredIntegratedPlugins)
            {
                PluginContainer.Create(integratedPlugin);
            }
        }

        // API data
        private static List<PluginPackageInfo> OnlinePlugins { get; set; }
        private static Dictionary<string, PluginPackageInfoCR> PluginsPackagesInfosCRs { get; set; } = new Dictionary<string, PluginPackageInfoCR>();

        public static PluginPackageInfoCR GetPluginPackageInfoCR(string pluginUUID)
        {
            if (PluginsPackagesInfosCRs.ContainsKey(pluginUUID)) return PluginsPackagesInfosCRs[pluginUUID];
            return null;
        }

        public static IEnumerable<PluginPackageInfoCR> RankedPlugins
        {
            get
            {
                return PluginsPackagesInfosCRs
                    .Where(kvp => !_integratedPlugins.Any(p => p.PluginUUID == kvp.Value.PluginUUID))
                    .Select(kvp => kvp.Value)
                    .OrderByDescending(info => info.HasNewerVersion)
                    .ThenByDescending(info => info.OnlineSupportedDeviceCount)
                    .ThenBy(info => info.PluginName);
            }
        }

#region Update miner plugin dlls
        public static async Task CheckAndSwapInstalledExternalPlugins()
        {
            try
            {
                if (ConfigManager.IsVersionChanged)
                {
                    string minerPluginsPath = Paths.MinerPluginsPath();
                    var zipPackages = Directory.GetFiles(Paths.RootPath("plugins_packages"), "*.zip", SearchOption.TopDirectoryOnly);
                    var installedExternalPackages = Directory.GetDirectories(minerPluginsPath);
                    foreach (var installedPath in installedExternalPackages)
                    {
                        try
                        {
                            var uuid = installedPath.Replace(minerPluginsPath, "").Trim('\\');
                            if (!System.Guid.TryParse(uuid, out var _)) continue;
                            var zipPackage = zipPackages.FirstOrDefault(package => package.Contains(uuid));
                            if (zipPackage == null) continue;
                            // uzip to temp dir
                            var tmpPluginDir = installedPath + "_tmp";
                            Directory.CreateDirectory(tmpPluginDir);
                            await ArchiveHelpers.ExtractFileAsync(zipPackage, tmpPluginDir, null, CancellationToken.None);
                            // now copy move over files 
                            var tmpPackageFiles = Directory.GetFiles(tmpPluginDir, "*", SearchOption.AllDirectories);
                            var installedPackagePaths = Directory.GetFiles(installedPath, "*", SearchOption.AllDirectories);
                            foreach (var path in installedPackagePaths)
                            {
                                // skip if not file and skip all bins
                                if (!File.Exists(path) || path.Contains("bins")) continue;
                                var fileName = Path.GetFileName(path);
                                var moveFile = tmpPackageFiles.FirstOrDefault(file => Path.GetFileName(file) == fileName);
                                if (moveFile == null) continue;
                                try
                                {
                                    File.Copy(moveFile, path, true);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error("CheckAndSwapInstalledExternalPlugins", e.Message);
                                }
                            }
                            Directory.Delete(tmpPluginDir, true);
                        }
                        catch (Exception e)
                        {
                            Logger.Error("CheckAndSwapInstalledExternalPlugins", e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("CheckAndSwapInstalledExternalPlugins", e.Message);
            }
        }



#endregion Update miner plugin dlls

        public static void LoadAndInitMinerPlugins()
        {
            // load dll's and create plugin containers
            var loadedPlugins = MinerPluginHost.LoadPlugins(Paths.MinerPluginsPath());
            foreach (var pluginUUID in loadedPlugins) PluginContainer.Create(MinerPluginHost.MinerPlugin[pluginUUID]);
            // init all containers
            foreach (var plugin in PluginContainer.PluginContainers)
            {
                if (!plugin.IsInitialized)
                {
                    plugin.InitPluginContainer();
                }

                if (plugin.Enabled)
                {
                    plugin.AddAlgorithmsToDevices();
                }
                else if (!plugin.IsCompatible)
                {
                    RemovePlugin(plugin.PluginUUID, false);
                }
                else
                {
                    plugin.RemoveAlgorithmsFromDevices();
                }
            }
            // cross reference local and online list
            CrossReferenceInstalledWithOnline();
            EthlargementIntegratedPlugin.Instance.ServiceEnabled = ConfigManager.GeneralConfig.UseEthlargement && Helpers.IsElevated;
            Logger.Info("MinerPluginsManager", "Finished initialization of miners.");
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

        public static async Task DownloadMissingMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatible)
                .Where(p => p.Enabled)
                .ToArray();

            foreach (var plugin in checkPlugins)
            {
                var urls = plugin.GetMinerBinsUrls().ToList();
                var missingFiles = plugin.CheckBinaryPackageMissingFiles();
                var hasMissingFiles = missingFiles.Any();
                var hasUrls = urls.Any();
                if (hasMissingFiles && hasUrls && !plugin.IsBroken)
                {
                    Logger.Info("MinerPluginsManager", $"Downloading missing files for {plugin.PluginUUID}-{plugin.Name}");
                    var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Downloading {0} %", $"{plugin.Name} {perc}"), perc)));
                    var unzipProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Unzipping {0} %", $"{plugin.Name} {perc}"), perc)));
                    await DownloadInternalBins(plugin.PluginUUID, urls.ToList(), downloadProgress, unzipProgress, stop);
                }
            }
        }

        public static async Task UpdateMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatible)
                .Where(p => p.Enabled)
                .ToArray();

            foreach (var plugin in checkPlugins)
            {
                var urls = plugin.GetMinerBinsUrls();
                var hasUrls = urls.Count() > 0;
                var versionMismatch = plugin.IsVersionMismatch;
                if (versionMismatch && hasUrls && !plugin.IsBroken)
                {
                    Logger.Info("MinerPluginsManager", $"Version mismatch for {plugin.PluginUUID}-{plugin.Name}. Downloading...");
                    var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Downloading {0} %", $"{plugin.Name} {perc}"), perc)));
                    var unzipProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Unzipping {0} %", $"{plugin.Name} {perc}"), perc)));
                    await DownloadInternalBins(plugin.PluginUUID, urls.ToList(), downloadProgress, unzipProgress, stop);
                }
            }
        }

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

        public static bool HasMinerUpdates()
        {
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatible)
                .Where(p => p.Enabled)
                .Where(p => p.IsVersionMismatch)
                .ToArray();


            return checkPlugins.Count() > 0;
        }

        public static void RemovePlugin(string pluginUUID, bool crossReferenceInstalledWithOnline = true)
        {
            try
            {
                var deletePath = Path.Combine(Paths.MinerPluginsPath(), pluginUUID);
                MinerPluginHost.MinerPlugin.Remove(pluginUUID);
                var oldPlugins = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUUID).ToArray();
                foreach (var old in oldPlugins)
                {
                    PluginContainer.RemovePluginContainer(old);
                }
                // TODO this remove is probably redundant CHECK
                foreach (var dev in AvailableDevices.Devices)
                {
                    dev.RemovePluginAlgorithms(pluginUUID);
                }

                // remove from cross ref dict
                if (PluginsPackagesInfosCRs.ContainsKey(pluginUUID))
                {
                    PluginsPackagesInfosCRs[pluginUUID].LocalInfo = null;
                    // TODO we might not have any online reference so remove it in this case
                    if (PluginsPackagesInfosCRs[pluginUUID].OnlineInfo == null)
                    {
                        PluginsPackagesInfosCRs.Remove(pluginUUID);
                    }
                }

                if (crossReferenceInstalledWithOnline) CrossReferenceInstalledWithOnline();
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

#warning "blocking method!!! Make it non blocking and change where it gets called"
        public static void CrossReferenceInstalledWithOnline()
        {
            // first go over the installed plugins
            // TODO rename installed to externalInstalledPlugin
            var checkPlugins = PluginContainer.PluginContainers
                //.Where(p => !p.IsIntegrated)
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
                if (PluginsPackagesInfosCRs.ContainsKey(uuid) == false)
                {
                    PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR{};
                }
                PluginsPackagesInfosCRs[uuid].LocalInfo = localPluginInfo;
            }

            // get online list and check what we have and what is online
            if (GetOnlineMinerPlugins() == false || OnlinePlugins == null) return;

            foreach (var online in OnlinePlugins)
            {
                var uuid = online.PluginUUID;
                if (PluginsPackagesInfosCRs.ContainsKey(uuid) == false)
                {
                    PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR{};
                }
                PluginsPackagesInfosCRs[uuid].OnlineInfo = online;
                if (online.SupportedDevicesAlgorithms != null)
                {
                    var supportedDevices = online.SupportedDevicesAlgorithms
                        .Where(kvp => kvp.Value.Count > 0)
                        .Select(kvp => kvp.Key);
                    var devRank = AvailableDevices.Devices
                        .Where(d => supportedDevices.Contains(d.DeviceType.ToString()))
                        .Count();
                    PluginsPackagesInfosCRs[uuid].OnlineSupportedDeviceCount = devRank;
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
            var ret = PluginContainer.PluginContainers.FirstOrDefault(p => p.PluginUUID == pluginUuid);
            return ret;
        }

#region DownloadingInstalling

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

        private static ConcurrentDictionary<string, MinerPluginInstallTask> MinerPluginInstallTasks = new ConcurrentDictionary<string, MinerPluginInstallTask>();

        public static void InstallAddProgress(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            if (MinerPluginInstallTasks.TryGetValue(pluginUUID, out var installTask))
            {
                installTask.AddProgress(progress);
            }
        }

        public static void InstallRemoveProgress(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            if (MinerPluginInstallTasks.TryGetValue(pluginUUID, out var installTask))
            {
                installTask.RemoveProgress(progress);
            }
        }

        public static void TryCancelInstall(string pluginUUID)
        {
            if (MinerPluginInstallTasks.TryGetValue(pluginUUID, out var installTask))
            {
                installTask.TryCancelInstall();
            }
        }

        public static async Task DownloadAndInstall(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            // TODO skip install if alredy in progress
            var addSuccess = false;
            using (var minerInstall = new MinerPluginInstallTask())
            {
                try
                {
                    var pluginPackageInfo = PluginsPackagesInfosCRs[pluginUUID];
                    addSuccess = MinerPluginInstallTasks.TryAdd(pluginUUID, minerInstall);
                    progress?.Report(Tuple.Create(PluginInstallProgressState.Pending, 0));
                    minerInstall.AddProgress(progress);
                    await DownloadAndInstall(pluginPackageInfo, minerInstall, minerInstall.CancelInstallToken);
                }
                finally
                {
                    if (addSuccess)
                    {
                        MinerPluginInstallTasks.TryRemove(pluginUUID, out var _);
                    }
                }
            }
        }

        internal static async Task DownloadAndInstall(PluginPackageInfoCR plugin, IProgress<Tuple<PluginInstallProgressState, int>> progress, CancellationToken stop)
        {
            var downloadPluginProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingPlugin, perc)));
            var zipProgressPluginChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingPlugin, perc)));
            var downloadMinerProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingMiner, perc)));
            var zipProgressMinerChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingMiner, perc)));

            var finalState = PluginInstallProgressState.Pending;
            const string installingPrefix = "installing_";
            var installingPluginPath = Path.Combine(Paths.MinerPluginsPath(), $"{installingPrefix}{plugin.PluginUUID}");
            try
            {
                if (Directory.Exists(installingPluginPath)) Directory.Delete(installingPluginPath, true);
                //downloadAndInstallUpdate("Starting");
                Directory.CreateDirectory(installingPluginPath);

                // download plugin dll
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingPlugin, 0));
                var downloadPluginResult = await MinersDownloadManager.DownloadFileAsync(plugin.PluginPackageURL, installingPluginPath, "plugin", downloadPluginProgressChangedEventHandler, stop);
                var pluginPackageDownloaded = downloadPluginResult.downloadedFilePath;
                var downloadPluginOK = downloadPluginResult.success;
                if (!downloadPluginOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingPlugin;
                    return;
                }
                // unzip 
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingExtractingPlugin, 0));
                var unzipPluginOK = await ArchiveHelpers.ExtractFileAsync(pluginPackageDownloaded, installingPluginPath, zipProgressPluginChangedEventHandler, stop);
                if (!unzipPluginOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedExtractingPlugin;
                    return;
                }
                File.Delete(pluginPackageDownloaded);

                // download plugin binary
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingMiner, 0));
                var downloadMinerBinsResult = await MinersDownloadManager.DownloadFileAsync(plugin.MinerPackageURL, installingPluginPath, "miner_bins", downloadMinerProgressChangedEventHandler, stop);
                var binsPackageDownloaded = downloadMinerBinsResult.downloadedFilePath;
                var downloadMinerBinsOK = downloadMinerBinsResult.success;
                if (!downloadMinerBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingMiner;
                    return;
                }
                // unzip 
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingExtractingMiner, 0));
                var binsUnzipPath = Path.Combine(installingPluginPath, "bins");
                var unzipMinerBinsOK = await ArchiveHelpers.ExtractFileAsync(binsPackageDownloaded, binsUnzipPath, zipProgressMinerChangedEventHandler, stop);
                if (!unzipMinerBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedExtractingMiner;
                    return;
                }
                File.Delete(binsPackageDownloaded);

                // TODO from here on add the failed plugin load state and success state
                var loadedPlugins = MinerPluginHost.LoadPlugin(installingPluginPath);
                if (loadedPlugins.Count() == 0)
                {
                    //downloadAndInstallUpdate($"Loaded ZERO PLUGINS");
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedPluginLoad;
                    Directory.Delete(installingPluginPath, true);
                    return;
                }

                //downloadAndInstallUpdate("Checking old plugin");
                var pluginPath = Path.Combine(Paths.MinerPluginsPath(), plugin.PluginUUID);
                // if there is an old plugin installed remove it
                if (Directory.Exists(pluginPath))
                {
                    // TODO consider saving the internal settings when updating the miner plugin
                    Directory.Delete(pluginPath, true);
                }
                //downloadAndInstallUpdate($"Loaded {loadedPlugins} PLUGIN");
                Directory.Move(installingPluginPath, pluginPath);
                // add or update plugins
                foreach (var pluginUUID in loadedPlugins)
                {
                    var newExternalPlugin = MinerPluginHost.MinerPlugin[pluginUUID];
                    // remove old
                    var oldPlugins = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUUID).ToArray();
                    foreach (var old in oldPlugins)
                    {
                        PluginContainer.RemovePluginContainer(old);
                    }
                    var newPlugin = PluginContainer.Create(newExternalPlugin);
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
                finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.Success;
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: ${e.Message}");
                //downloadAndInstallUpdate();
                finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedUnknown;
            }
            finally
            {
                progress?.Report(Tuple.Create(finalState, 0));
            }
        }
#endregion DownloadingInstalling
    }
}
