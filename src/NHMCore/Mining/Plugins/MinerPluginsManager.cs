using NHM.MinerPlugin;
using NHM.MinerPluginLoader;
using NHM.MinerPluginToolkitV1;
using Newtonsoft.Json;
using NHM.Common;
using NHM.MinersDownloader;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Notifications;
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
using NHM.MinerPluginToolkitV1.Interfaces;
using NHM.Common.Enums;

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
                // __DEV__*          
#if INTEGRATE_BrokenMiner_PLUGIN
                new BrokenMiner.BrokenMinerPlugin(),
#endif
#if INTEGRATE_ExamplePlugin_PLUGIN
                new Example.ExamplePlugin(),
#endif
#if INTEGRATE_FakePlugin_PLUGIN
                new FakePlugin.FakePlugin(),
#endif

// real miners
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
#if INTEGRATE_NanoMiner_PLUGIN
                new NanoMiner.NanoMinerPlugin(),
#endif
#if INTEGRATE_WildRig_PLUGIN
                new WildRig.WildRigPlugin(),
#endif
#if INTEGRATE_CryptoDredge_PLUGIN
                new CryptoDredge.CryptoDredgePlugin(),
#endif
#if INTEGRATE_ZEnemy_PLUGIN
                new ZEnemy.ZEnemyPlugin(),
#endif
#if INTEGRATE_LolMiner_PLUGIN
                new LolMiner.LolMinerPlugin(),
#endif
#if INTEGRATE_SRBMiner_PLUGIN
                new SRBMiner.SRBMinerPlugin(),
#endif
#if INTEGRATE_XMRig_PLUGIN
                new XMRig.XMRigPlugin(),
#endif
#if INTEGRATE_MiniZ_PLUGIN
                new MiniZ.MiniZPlugin(),
#endif

#if INTEGRATE_ALL_PLUGINS
                new GMinerPlugin.GMinerPlugin(),
                new NBMiner.NBMinerPlugin(),
                new Phoenix.PhoenixPlugin(),
                new TeamRedMiner.TeamRedMinerPlugin(),
                new TRex.TRexPlugin(),
                new TTMiner.TTMinerPlugin(),
                new NanoMiner.NanoMinerPlugin(),
                new WildRig.WildRigPlugin(),
                new CryptoDredge.CryptoDredgePlugin(),
                new ZEnemy.ZEnemyPlugin(),
                new LolMiner.LolMinerPlugin(),
                new SRBMiner.SRBMinerPlugin(),
                new MiniZ.MiniZPlugin(),
#endif


                // service plugin
                EthlargementIntegratedPlugin.Instance,

                // plugin dependencies
                VC_REDIST_x64_2015_2019_DEPENDENCY_PLUGIN.Instance
            };
            var filteredIntegratedPlugins = _integratedPlugins.Where(p => BlacklistedPlugins.IsNotBlacklisted(p.PluginUUID)).ToList();
            foreach (var integratedPlugin in filteredIntegratedPlugins)
            {
                PluginContainer.Create(integratedPlugin);
            }

            (_initOnlinePlugins, OnlinePlugins) = ReadCachedOnlinePlugins();
        }

        private static readonly bool _initOnlinePlugins;

        // API data
        private static List<PluginPackageInfo> OnlinePlugins { get; set; }
        private static Dictionary<string, PluginPackageInfoCR> PluginsPackagesInfosCRs { get; set; } = new Dictionary<string, PluginPackageInfoCR>();

        //public static PluginPackageInfoCR GetPluginPackageInfoCR(string pluginUUID)
        //{
        //    if (PluginsPackagesInfosCRs.ContainsKey(pluginUUID)) return PluginsPackagesInfosCRs[pluginUUID];
        //    return null;
        //}

        public static IEnumerable<PluginPackageInfoCR> RankedPlugins
        {
            get
            {
                return PluginsPackagesInfosCRs
                    .Select(kvp => kvp.Value)
                    .OrderByDescending(info => info.HasNewerVersion)
                    .ThenByDescending(info => info.OnlineSupportedDeviceCount)
                    .ThenBy(info => info.PluginName);
            }
        }

        #region Update miner plugin dlls
        public static void CheckAndDeleteUnsupportedPlugins()
        {
            try
            {
                foreach (var obsolete in Checkers.ObsoleteMinerPlugins)
                {
                    try
                    {
                        var obsoletePath = Paths.MinerPluginsPath(obsolete);
                        if (Directory.Exists(obsoletePath))
                        {
                            Directory.Delete(obsoletePath, true);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("CheckAndDeleteUnsupportedPlugins", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("CheckAndDeleteUnsupportedPlugins", e.Message);
            }
        }

        #endregion Update miner plugin dlls

        public static async Task LoadAndInitMinerPlugins()
        {
            try
            {
                // TODO ADD STEP AND MESSAGE
                EthlargementIntegratedPlugin.Instance.InitAndCheckSupportedDevices(AvailableDevices.Devices.Select(dev => dev.BaseDevice));
                CheckAndDeleteUnsupportedPlugins();

                // load dll's and create plugin containers
                var loadedPlugins = MinerPluginHost.LoadPlugins(Paths.MinerPluginsPath()).Where(uuid => BlacklistedPlugins.IsNotBlacklisted(uuid));
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
                    else
                    {
                        plugin.RemoveAlgorithmsFromDevices();
                    }
                }
                // cross reference local and online list
                var success = await GetOnlineMinerPlugins();
                if (success) CrossReferenceInstalledWithOnline();
                EthlargementIntegratedPlugin.Instance.ServiceEnabled = MiscSettings.Instance.UseEthlargement && Helpers.IsElevated;
                CheckAccepted3rdPartyPlugins();
                Logger.Info("MinerPluginsManager", "Finished initialization of miners.");
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Initialization of miners error {e}.");
            }
        }

        public static Task RunninLoops { get; private set; } = null;

        public static void StartLoops(CancellationToken stop)
        {
            RunninLoops = Task.Run(() => {
                var loop1 = MainLoop(stop);
                return Task.WhenAll(loop1);
            });
        }

        private static async Task MainLoop(CancellationToken stop)
        {
            try
            {
                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                Func<bool> isActive = () => !stop.IsCancellationRequested;


                // TODO set this interval somwhere
                // periodically update the plugin list
                var getOnlineMinerPluginsElapsedTimeChecker = new ElapsedTimeChecker(MinerPluginsUpdaterSettings.CheckPluginsInterval, false);


                // TODO for now every minute check
                // TODO debug only we should check plugin updates after we update the miner plugin API
                //var pluginsUpdateElapsedTimeChecker = new ElapsedTimeChecker(TimeSpan.FromSeconds(30), false);

                var restartActiveDevicesPluginsList = new List<string>();

                Logger.Debug("MinerPluginsManager", $"STARTING MAIN LOOP");
                while (isActive())
                {
                    try
                    {
                        if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);
                        
                        var execAutoUpdate = false;
                        if (getOnlineMinerPluginsElapsedTimeChecker.CheckAndMarkElapsedTime())
                        {
                            Logger.Debug("MinerPluginsManager", $"Checking for plugin updates");
                            var success = await GetOnlineMinerPlugins();
                            if (success) CrossReferenceInstalledWithOnline();
                            var logValue = success ? "SUCCESS" : "FAIL";
                            Logger.Debug("MinerPluginsManager", $"Checking for plugin updates returned {logValue}");
                            execAutoUpdate = success && UpdateSettings.Instance.AutoUpdateMinerPlugins;
                        }

                        if (isActive() && execAutoUpdate)
                        {
                            Logger.Debug("MinerPluginsManager", $"Checking plugins to Install/Update");
                            var pluginsThatCanAutoUpdate = PluginsPackagesInfosCRs.Values
                                .Where(p => p.Installed)
                                .Where(p => p.IsAutoUpdateEnabled)
                                .Where(p => p.HasNewerVersion)
                                .Where(p => p.CompatibleNHPluginVersion)
                                .Where(p => p.Supported)
                                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                                .Where(p => MinerPluginInstallTasks.ContainsKey(p.PluginUUID) == false) // skip if update is already in progress
                                .Select(p => p.PluginUUID)
                                .ToArray();
                            foreach (var pluginUUID in pluginsThatCanAutoUpdate)
                            {
                                Logger.Debug("MinerPluginsManager", $"Main loop Install/Update {pluginUUID}");
                                _ = _minerPluginInstallTasksProgress.TryGetValue(pluginUUID, out var progress);
                                _ = DownloadAndInstall(pluginUUID, progress);
                            }
                        }

                        // TODO trigger active device re-evaluation after install/remove/updates are finished
                        if (isActive() && (_minerPluginInstallRemoveStates.Count + restartActiveDevicesPluginsList.Count) > 0)
                        {
                            var finishedKeys = new List<string>();
                            foreach (var kvp in _minerPluginInstallRemoveStates)
                            {
                                if (kvp.Value != PluginInstallRemoveState.Remove && kvp.Value != PluginInstallRemoveState.InstallOrUpdate)
                                {
                                    restartActiveDevicesPluginsList.Add(kvp.Key);
                                    finishedKeys.Add(kvp.Key);
                                }
                                Logger.DebugDelayed("MinerPluginsManager", $"_minerPluginInstallRemoveStates {kvp.Key}-{kvp.Value.ToString()}", TimeSpan.FromSeconds(5));
                            }
                            var allRemoved = true;
                            foreach (var key in finishedKeys)
                            {
                                allRemoved &= _minerPluginInstallRemoveStates.TryRemove(key, out var _);
                            }
                            if (!allRemoved)
                            {
                                Logger.DebugDelayed("MinerPluginsManager", $"_minerPluginInstallRemoveStates allRemoved false", TimeSpan.FromSeconds(5));
                            }
                            if (_minerPluginInstallRemoveStates.Count == 0)
                            {
                                restartActiveDevicesPluginsList.Clear();
                                Logger.DebugDelayed("MinerPluginsManager", $"RESTART ACTIVE DEVICES", TimeSpan.FromSeconds(5));
                                _ = ApplicationStateManager.RestartDevicesState();
                            }
                            else
                            {
                                Logger.DebugDelayed("MinerPluginsManager", $"SKIP!!!!!!!!!!!!!!! RESTART ACTIVE DEVICES", TimeSpan.FromSeconds(5));
                            }
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        Logger.Info("MinerPluginsManager", $"Main Loop TaskCanceledException {e.Message}");
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("MinerPluginsManager", $"Main Loop Tick Exception {e.Message}");
                    }
                }
            }
            finally
            {
                Logger.Debug("MinerPluginsManager", $"EXITING MAIN LOOP");
                // cleanup
                var pluginUUIDs = MinerPluginInstallTasks.Keys;
                foreach (var pluginUUID in pluginUUIDs)
                {
                    TryCancelInstall(pluginUUID);
                }
            }
        }

        public static async Task DevicesCrossReferenceIDsWithMinerIndexes(IStartupLoader loader)
        {
            // get devices
            var baseDevices = AvailableDevices.Devices.Select(dev => dev.BaseDevice);
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                //.Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID)) // WARNING We still want to mine with these 
                .ToArray();

            if (checkPlugins.Length > 0 && loader != null) {
                loader.SecondaryVisible = true;
                loader.SecondaryTitle = Translations.Tr("Devices Cross Reference");
                loader?.SecondaryProgress?.Report((Translations.Tr("Pending"), 0));
            }
            var pluginDoneCount = 0d;
            foreach (var plugin in checkPlugins)
            {
                loader?.SecondaryProgress?.Report((Translations.Tr("Cross Reference {0}", plugin.Name), (int)((pluginDoneCount / checkPlugins.Length) * 100)));
                await plugin.DevicesCrossReference(baseDevices);
                pluginDoneCount += 1;
                loader?.SecondaryProgress?.Report((Translations.Tr("Cross Reference {0}", plugin.Name), (int)((pluginDoneCount / checkPlugins.Length) * 100)));
            }
            if (loader != null)
            {
                loader.SecondaryVisible = false;
            }
        }

        public static async Task DownloadMissingMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            var pluginsWithMissingPackageFiles = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(p => p.HasMisingBinaryPackageFiles())
                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                .Select(p => (p, p.GetMinerBinsUrls().ToList()))
                .Where<(PluginContainer p, List<string> urls)>(pair => pair.urls.Any())
                .ToArray();

            foreach (var (plugin, urls) in pluginsWithMissingPackageFiles)
            {
                Logger.Info("MinerPluginsManager", $"Downloading missing files for {plugin.PluginUUID}-{plugin.Name}");
                var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Downloading {0} %", $"{plugin.Name} {perc}"), perc)));
                var unzipProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Extracting {0} %", $"{plugin.Name} {perc}"), perc)));
                await DownloadInternalBins(plugin, urls, downloadProgress, unzipProgress, stop);
                // check if we have missing files after the download 
                if (plugin.HasMisingBinaryPackageFiles()) AvailableNotifications.CreateMissingMinerBinsInfo(plugin.Name);
            }
        }

        private static string PluginInstallProgressStateToString(PluginInstallProgressState state, string pluginName, int progressPerc) {
            switch (state)
            {
                case PluginInstallProgressState.DownloadingMiner:
                    return Translations.Tr("Downloading Miner: {0}%", $"{pluginName} {progressPerc}");
                case PluginInstallProgressState.DownloadingPlugin:
                    return Translations.Tr("Downloading Plugin: {0}%", $"{pluginName} {progressPerc}");
                case PluginInstallProgressState.ExtractingMiner:
                    return Translations.Tr("Extracting Miner: {0}%", $"{pluginName} {progressPerc}");
                case PluginInstallProgressState.ExtractingPlugin:
                    return Translations.Tr("Extracting Plugin: {0}%", $"{pluginName} {progressPerc}");
                default:
                    return Translations.Tr("Pending Install") + $" {pluginName}";
            }
        }


        public static async Task UpdateMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            Func<PluginContainer, bool> hasUpdate = (p) => {
                return PluginsPackagesInfosCRs.TryGetValue(p.PluginUUID, out var pcr) && pcr.HasNewerVersion;
            };
            var pluginsToUpdate = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(hasUpdate)
                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                .ToArray();

            foreach (var plugin in pluginsToUpdate)
            {
                var wrappedProgress = new Progress<Tuple<PluginInstallProgressState, int>>(status =>
                {
                    var (state, progressPerc) = status;
                    string statusText = PluginInstallProgressStateToString(state, plugin.Name, progressPerc); 
                    progress?.Report((statusText, progressPerc));
                });
                await DownloadAndInstall(plugin.PluginUUID, wrappedProgress);
            }
        }

        internal static bool CanGetPluginVersionAndUUID(IMinerPlugin plugin)
        {
            try
            {
                var uuid = plugin.PluginUUID;
                var version = plugin.Version;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static bool PluginHasMinerBins(IMinerPlugin plugin)
        {
            try
            {
                return plugin is IBinaryPackageMissingFilesChecker impl && impl.CheckBinaryPackageMissingFiles().Count() == 0;
            }
            catch (Exception)
            {}
            return false;
        }

        private static void CheckAccepted3rdPartyPlugins()
        {
            var nonAcceptedlugins = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(p => !AcceptedPlugins.IsAccepted(p.PluginUUID))
                .ToArray();
            var nonAcceptedluginsUUIDs = nonAcceptedlugins
                .Select(p => p.PluginUUID)
                .ToArray();
            EulaConfirm = RankedPlugins.Where(pcr => nonAcceptedluginsUUIDs.Contains(pcr.PluginUUID)).ToList();
            EulaConfirm.ForEach(el => {
                Logger.Info("MinerPluginsManager", $"Plugin is not accepted {el.PluginUUID}-{el.PluginName}. Skipping...");
                el.IsUserActionRequired = true;
            });
            var nonAcceptedPluginsWithMissingBinaries = nonAcceptedlugins.Where(p => p.HasMisingBinaryPackageFiles()).ToArray();
            foreach (var p in nonAcceptedPluginsWithMissingBinaries)
            {
                try
                {
                    var oldPlugins = Directory.GetFiles(Paths.MinerPluginsPath(p.PluginUUID, "dlls"), "*.dll", SearchOption.AllDirectories)
                        .SelectMany(MinerPluginHost.LoadPluginsFromDllFile)
                        .Where(CanGetPluginVersionAndUUID)
                        .Where(plugin => plugin.PluginUUID == p.PluginUUID)
                        .Where(PluginHasMinerBins)
                        .OrderByDescending(plugin => plugin.Version)
                        .ToList();
                    if (oldPlugins.Count > 0) PluginContainer.RemovePluginContainer(p);
                    foreach (var fallbackPlugin in oldPlugins)
                    {
                        // init all containers
                        var plugin = PluginContainer.Create(fallbackPlugin);
                        if (!plugin.IsInitialized)
                        {
                            plugin.InitPluginContainer();
                        }

                        if (plugin.Enabled)
                        {
                            plugin.AddAlgorithmsToDevices();
                            break; // we are good stop fallback plugins init
                        }
                        else
                        {
                            plugin.RemoveAlgorithmsFromDevices();
                            PluginContainer.RemovePluginContainer(plugin);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("MinerPluginsManager", $"Error setting fallback plugin '{p.PluginUUID}': {e.Message}");
                }
            }
        }

        public static List<PluginPackageInfoCR> EulaConfirm { get; private set; } = new List<PluginPackageInfoCR>();

        public static bool HasMissingMiners()
        {
            var anyPluginWithMissingPackageFiles = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                .Any(p => p.HasMisingBinaryPackageFiles());
            return anyPluginWithMissingPackageFiles;
        }

        public static void RemovePlugin(string pluginUUID, bool crossReferenceInstalledWithOnline = true)
        {
            // assume ok
            var isOk = true;
            try
            {
                AcceptedPlugins.Remove(pluginUUID);
                if (EthlargementIntegratedPlugin.Instance.PluginUUID == pluginUUID) EthlargementIntegratedPlugin.Instance.Remove();
                _minerPluginInstallRemoveStates.TryAdd(pluginUUID, PluginInstallRemoveState.Remove);
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
                isOk = false;
                Logger.Error("MinerPluginsManager", $"Error occured while removing {pluginUUID} plugin: {e.Message}");
            }
            finally
            {
                if (isOk)
                {
                    BlacklistedPlugins.AddToBlacklist(pluginUUID);
                    _minerPluginInstallRemoveStates.TryUpdate(pluginUUID, PluginInstallRemoveState.RemoveSuccess, PluginInstallRemoveState.Remove);
                }
                else
                {
                    _minerPluginInstallRemoveStates.TryUpdate(pluginUUID, PluginInstallRemoveState.RemoveFailed, PluginInstallRemoveState.Remove);
                }
            }
        }

        public static void CrossReferenceInstalledWithOnline()
        {
            // EthlargementIntegratedPlugin scope special case
            {
                var uuid = EthlargementIntegratedPlugin.Instance.PluginUUID;
                if (PluginsPackagesInfosCRs.ContainsKey(uuid) == false)
                {
                    PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR(uuid);
                }
                var ethlargementPluginContainer = PluginContainer.PluginContainers.FirstOrDefault(p => p.PluginUUID == EthlargementIntegratedPlugin.Instance.PluginUUID);
                if (EthlargementIntegratedPlugin.Instance.IsInstalled)
                {
                    var localPluginInfo = new PluginPackageInfo
                    {
                        PluginAuthor = EthlargementIntegratedPlugin.Instance.Author,
                        PluginName = EthlargementIntegratedPlugin.Instance.Name,
                        PluginUUID = uuid,
                        PluginVersion = EthlargementIntegratedPlugin.Instance.Version,
                        // other stuff is not inside the plugin
                    };
                    PluginsPackagesInfosCRs[uuid].LocalInfo = localPluginInfo;
                }
                var online = new PluginPackageInfo
                {
                    PluginAuthor = EthlargementIntegratedPlugin.Instance.Author,
                    PluginName = EthlargementIntegratedPlugin.Instance.Name,
                    PluginUUID = uuid,
                    PluginVersion = EthlargementIntegratedPlugin.Instance.Version,
                    MinerPackageURL = EthlargementIntegratedPlugin.Instance.GetMinerBinsUrlsForPlugin().FirstOrDefault(),
                    PackagePassword = null,
                    PluginDescription = "ETHlargement increases DaggerHashimoto hashrate for NVIDIA 1080, 1080 Ti and Titan Xp GPUs.",
                    PluginPackageURL = "N/A",
                    SupportedDevicesAlgorithms = new Dictionary<string, List<string>> {
                        { DeviceType.NVIDIA.ToString(), new List<string> { AlgorithmType.DaggerHashimoto.ToString() } }
                    }
                };
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

            // first go over the installed plugins
            var installedPlugins = PluginContainer.PluginContainers
                //.Where(p => p.Enabled) // we can have installed plugins that are obsolete
                .Where(p => !_integratedPlugins.Any(integrated => integrated.PluginUUID == p.PluginUUID)) // ignore integrated
                .ToArray();
            foreach (var installed in installedPlugins)
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
                    PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR(uuid);
                }
                PluginsPackagesInfosCRs[uuid].LocalInfo = localPluginInfo;
            }

            // get online list and check what we have and what is online
            if (OnlinePlugins == null) return;

            foreach (var online in OnlinePlugins)
            {
                var uuid = online.PluginUUID;
                if (PluginsPackagesInfosCRs.ContainsKey(uuid) == false)
                {
                    PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR(uuid);
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

            MinerPluginsManagerState.Instance.RankedPlugins = RankedPlugins.ToList();
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


        private static async Task<bool> GetOnlineMinerPlugins()
        {
            try
            {
                using (var client = new NoKeepAlivesWebClient())
                {
                    string s = await client.DownloadStringTaskAsync(Links.PluginsJsonApiUrl);
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
            return _initOnlinePlugins;
        }

        private static (bool, List<PluginPackageInfo>) ReadCachedOnlinePlugins()
        {
            try
            {
                string s = File.ReadAllText(Paths.RootPath("plugins_packages", "update.json"));

                var onlinePlugins = JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Culture = CultureInfo.InvariantCulture
                });
                return (true, onlinePlugins);
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Error occured while reading cached online miner plugins: {e.Message}");
            }
            return (false, null);
        }

        public static PluginContainer GetPluginWithUuid(string pluginUuid)
        {
            var ret = PluginContainer.PluginContainers.FirstOrDefault(p => p.PluginUUID == pluginUuid);
            return ret;
        }

#region DownloadingInstalling

        public static async Task DownloadInternalBins(PluginContainer pluginContainer, List<string> urls, IProgress<int> downloadProgress, IProgress<int> unzipProgress, CancellationToken stop)
        {
            if (EthlargementIntegratedPlugin.Instance.PluginUUID == pluginContainer.PluginUUID)
            {
                await DownloadEthlargementInternalBins(urls, downloadProgress, stop);
                return;
            }
            var pluginUUID = pluginContainer.PluginUUID;
            var ver = pluginContainer.Version;
            var installingPluginBinsPath = Path.Combine(Paths.MinerPluginsPath(), pluginUUID, "bins", $"{ver.Major}.{ver.Minor}");
            try
            {
                if (Directory.Exists(installingPluginBinsPath)) Directory.Delete(installingPluginBinsPath, true);
                Directory.CreateDirectory(installingPluginBinsPath);
                var installedBins = false;
                foreach (var url in urls)
                {
                    var downloadMinerBinsResult = await MinersDownloadManager.DownloadFileAsync(url, installingPluginBinsPath, "miner_bins", downloadProgress, stop);
                    var binsPackageDownloaded = downloadMinerBinsResult.downloadedFilePath;
                    var downloadMinerBinsOK = downloadMinerBinsResult.success;
                    if (!downloadMinerBinsOK || stop.IsCancellationRequested) return;
                    // unzip 
                    var binsUnzipPath = installingPluginBinsPath;
                    var unzipMinerBinsOK = await ArchiveHelpers.ExtractFileAsync(pluginContainer.GetBinsPackagePassword(), binsPackageDownloaded, binsUnzipPath, unzipProgress, stop);
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

                //clear old bins
                clearOldPluginBins(Path.Combine(Paths.MinerPluginsPath(), pluginUUID, "bins"));
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Installation of {pluginUUID} failed: ${e.Message}");
            }
        }

        private static async Task<bool> DownloadEthlargementInternalBins(List<string> urls, IProgress<int> downloadProgress, CancellationToken stop)
        {
            var pluginUUID = EthlargementIntegratedPlugin.Instance.PluginUUID;
            var ver = EthlargementIntegratedPlugin.Instance.Version;
            var installingPluginBinsPath = Path.Combine(Paths.MinerPluginsPath(), pluginUUID, "bins", $"{ver.Major}.{ver.Minor}");
            try
            {
                if (Directory.Exists(installingPluginBinsPath)) Directory.Delete(installingPluginBinsPath, true);
                Directory.CreateDirectory(installingPluginBinsPath);
                var installedBins = false;
                foreach (var url in urls)
                {
                    var downloadMinerBinsResult = await MinersDownloadManager.DownloadFileAsync(url, installingPluginBinsPath, EthlargementIntegratedPlugin.BinName.Replace(".exe", ""), downloadProgress, stop);
                    var binsPackageDownloaded = downloadMinerBinsResult.downloadedFilePath;
                    var downloadMinerBinsOK = downloadMinerBinsResult.success;
                    installedBins = true;
                    if (!downloadMinerBinsOK || stop.IsCancellationRequested) return false;
                    if (stop.IsCancellationRequested) return false;
                    
                    break;
                }
                if (!installedBins)
                {
                    Logger.Error("DownloadEthlargementInternalBins", $"Miners bins of {pluginUUID} not installed");
                }

                //clear old bins
                clearOldPluginBins(Path.Combine(Paths.MinerPluginsPath(), pluginUUID, "bins"));
                return installedBins;
            }
            catch (Exception e)
            {
                Logger.Error("DownloadEthlargementInternalBins", $"Installation of {pluginUUID} failed: ${e.Message}");
                return false;
            }
        }

        private static ConcurrentDictionary<string, MinerPluginInstallTask> MinerPluginInstallTasks = new ConcurrentDictionary<string, MinerPluginInstallTask>();
        // with WPF we have only one Progress 
        private static ConcurrentDictionary<string, IProgress<Tuple<PluginInstallProgressState, int>>> _minerPluginInstallTasksProgress = new ConcurrentDictionary<string, IProgress<Tuple<PluginInstallProgressState, int>>>();
        
        private static ConcurrentDictionary<string, PluginInstallRemoveState> _minerPluginInstallRemoveStates = new ConcurrentDictionary<string, PluginInstallRemoveState>();

        public static void InstallAddProgress(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            if (MinerPluginInstallTasks.TryGetValue(pluginUUID, out var installTask))
            {
                installTask.AddProgress(progress);
            }
            // 
            if (_minerPluginInstallTasksProgress.ContainsKey(pluginUUID) == false)
            {
                _minerPluginInstallTasksProgress.TryAdd(pluginUUID, progress);
            }
        }

        public static void InstallRemoveProgress(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            if (MinerPluginInstallTasks.TryGetValue(pluginUUID, out var installTask))
            {
                installTask.RemoveProgress(progress);
            }
            if (_minerPluginInstallTasksProgress.TryRemove(pluginUUID, out var _) == false)
            {
                // log error
            }
        }

        public static void TryCancelInstall(string pluginUUID)
        {
            if (MinerPluginInstallTasks.TryRemove(pluginUUID, out var installTask))
            {
                installTask.TryCancelInstall();
            }
        }

        public static async Task DownloadAndInstall(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            // TODO skip install if alredy in progress
            var addSuccess = false;
            _minerPluginInstallRemoveStates.TryAdd(pluginUUID, PluginInstallRemoveState.InstallOrUpdate);
            var installResult = PluginInstallProgressState.Canceled;
            using (var minerInstall = new MinerPluginInstallTask())
            {
                try
                {
                    var pluginPackageInfo = PluginsPackagesInfosCRs[pluginUUID];
                    addSuccess = MinerPluginInstallTasks.TryAdd(pluginUUID, minerInstall);
                    if (progress != null)
                    {
                        progress?.Report(Tuple.Create(PluginInstallProgressState.Pending, 0));
                        minerInstall.AddProgress(progress);
                    }
                    if (pluginPackageInfo.PluginUUID == EthlargementIntegratedPlugin.Instance.PluginUUID)
                    {
                        installResult = await DownloadAndInstallEthlargementIntegratedPlugin(pluginPackageInfo, minerInstall, minerInstall.CancelInstallToken);
                    }
                    else
                    {
                        installResult = await DownloadAndInstall(pluginPackageInfo, minerInstall, minerInstall.CancelInstallToken);
                    }
                }
                finally
                {
                    if (addSuccess)
                    {
                        BlacklistedPlugins.RemoveFromBlacklist(pluginUUID);
                        MinerPluginInstallTasks.TryRemove(pluginUUID, out var _);
                    }
                    if (installResult == PluginInstallProgressState.Success)
                    {
                        AvailableNotifications.CreatePluginUpdateInfo(PluginsPackagesInfosCRs[pluginUUID].PluginName, true);
                        _minerPluginInstallRemoveStates.TryUpdate(pluginUUID, PluginInstallRemoveState.InstallOrUpdateSuccess, PluginInstallRemoveState.InstallOrUpdate);
                    }
                    else
                    {
                        AvailableNotifications.CreatePluginUpdateInfo(PluginsPackagesInfosCRs[pluginUUID].PluginName, false);
                        _minerPluginInstallRemoveStates.TryUpdate(pluginUUID, PluginInstallRemoveState.InstallOrUpdateFailed, PluginInstallRemoveState.InstallOrUpdate);
                    }
                }
            }
        }



        internal static async Task<PluginInstallProgressState> DownloadAndInstall(PluginPackageInfoCR plugin, IProgress<Tuple<PluginInstallProgressState, int>> progress, CancellationToken stop)
        {
            var downloadPluginProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingPlugin, perc)));
            var zipProgressPluginChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingPlugin, perc)));
            var downloadMinerProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingMiner, perc)));
            var zipProgressMinerChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingMiner, perc)));

            var finalState = PluginInstallProgressState.Pending;
            try
            {
                var versionStr = $"{plugin.OnlineInfo.PluginVersion.Major}.{plugin.OnlineInfo.PluginVersion.Minor}";
                var pluginRootPath = Path.Combine(Paths.MinerPluginsPath(), plugin.PluginUUID);
                var installDllPath = Path.Combine(pluginRootPath, "dlls", versionStr);
                var installBinsPath = Path.Combine(pluginRootPath, "bins", versionStr);

                if (Directory.Exists(installDllPath)) Directory.Delete(installDllPath, true);
                Directory.CreateDirectory(installDllPath);
                if (Directory.Exists(installBinsPath)) Directory.Delete(installBinsPath, true);
                Directory.CreateDirectory(installBinsPath);

                //clear old bins
                clearOldPluginBins(Path.Combine(pluginRootPath, "bins"));

                // download plugin dll
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingPlugin, 0));
                var downloadPluginResult = await MinersDownloadManager.DownloadFileAsync(plugin.PluginPackageURL, installDllPath, "plugin", downloadPluginProgressChangedEventHandler, stop);
                var pluginPackageDownloaded = downloadPluginResult.downloadedFilePath;
                var downloadPluginOK = downloadPluginResult.success;
                if (!downloadPluginOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingPlugin;
                    return finalState;
                }
                // unzip 
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingExtractingPlugin, 0));
                var unzipPluginOK = await ArchiveHelpers.ExtractFileAsync(null, pluginPackageDownloaded, installDllPath, zipProgressPluginChangedEventHandler, stop);
                if (!unzipPluginOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedExtractingPlugin;
                    return finalState;
                }
                File.Delete(pluginPackageDownloaded);

                // download plugin binary
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingMiner, 0));
                var downloadMinerBinsResult = await MinersDownloadManager.DownloadFileAsync(plugin.MinerPackageURL, installBinsPath, "miner_bins", downloadMinerProgressChangedEventHandler, stop);
                var binsPackageDownloaded = downloadMinerBinsResult.downloadedFilePath;
                var downloadMinerBinsOK = downloadMinerBinsResult.success;
                if (!downloadMinerBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingMiner;
                    return finalState;
                }
                // unzip 
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingExtractingMiner, 0));
                var unzipMinerBinsOK = await ArchiveHelpers.ExtractFileAsync(plugin.BinsPackagePassword, binsPackageDownloaded, installBinsPath, zipProgressMinerChangedEventHandler, stop);
                if (!unzipMinerBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedExtractingMiner;
                    return finalState;
                }
                File.Delete(binsPackageDownloaded);

                // TODO from here on add the failed plugin load state and success state
                var loadedPlugins = MinerPluginHost.LoadPlugin(installDllPath);
                if (loadedPlugins.Count() == 0)
                {
                    //downloadAndInstallUpdate($"Loaded ZERO PLUGINS");
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedPluginLoad;
                    Directory.Delete(installDllPath, true);
                    return finalState;
                }

                //downloadAndInstallUpdate("Checking old plugin");
                //downloadAndInstallUpdate($"Loaded {loadedPlugins} PLUGIN");
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
                    // TODO/TESTING scope for our fake plugins
                    try
                    {
                        var newPluginDllSettings = Directory.GetFiles(installDllPath, "*.json");
                        foreach (var jsonFile in newPluginDllSettings)
                        {
                            var name = Path.GetFileName(jsonFile);
                            var installJSONFilePath = Path.Combine(pluginRootPath, name);
                            File.Copy(jsonFile, installJSONFilePath, true);
                        }
                    }
                    catch { }
                    var success = newPlugin.InitPluginContainer();
                    // TODO after add or remove plugins we should clean up the device settings
                    if (success)
                    {
                        var oldInstalledDlls = Directory.GetFiles(pluginRootPath, "*.dll");
                        foreach (var oldDll in oldInstalledDlls)
                        {
                            File.Delete(oldDll);
                        }
                        var newDllPath = Directory.GetFiles(installDllPath).FirstOrDefault();
                        var name = Path.GetFileNameWithoutExtension(newDllPath);
                        var newVerStr = $"{newPlugin.Version.Major}.{newPlugin.Version.Minor}";
                        var installedDllPath = Path.Combine(pluginRootPath, $"{name}.dll");
                        File.Copy(newDllPath, installedDllPath);

                        newPlugin.AddAlgorithmsToDevices();
                        await newPlugin.DevicesCrossReference(AvailableDevices.Devices.Select(d => d.BaseDevice));
                        finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.Success;
                    }
                    else
                    {
                        finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedPluginInit;
                        // TODO mark that this plugin wasn't loaded
                        Logger.Error("MinerPluginsManager", $"DownloadAndInstall unable to init and install {pluginUUID}");
                    }
                }
                // cross reference local and online list
                CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: {e.Message}");
                //downloadAndInstallUpdate();
                finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedUnknown;
            }
            finally
            {
                progress?.Report(Tuple.Create(finalState, 0));
            }
            return finalState;
        }
        
        internal static async Task<PluginInstallProgressState> DownloadAndInstallEthlargementIntegratedPlugin(PluginPackageInfoCR plugin, IProgress<Tuple<PluginInstallProgressState, int>> progress, CancellationToken stop)
        {
            var downloadPluginProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingPlugin, perc)));
            var zipProgressPluginChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingPlugin, perc)));
            var downloadMinerProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingMiner, perc)));
            var zipProgressMinerChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingMiner, perc)));

            var finalState = PluginInstallProgressState.Pending;
            try
            {
                var versionStr = $"{plugin.OnlineInfo.PluginVersion.Major}.{plugin.OnlineInfo.PluginVersion.Minor}";
                var pluginRootPath = Path.Combine(Paths.MinerPluginsPath(), plugin.PluginUUID);
                var installDllPath = Path.Combine(pluginRootPath, "dlls", versionStr);
                var installBinsPath = Path.Combine(pluginRootPath, "bins", versionStr);

                if (Directory.Exists(installDllPath)) Directory.Delete(installDllPath, true);
                Directory.CreateDirectory(installDllPath);
                if (Directory.Exists(installBinsPath)) Directory.Delete(installBinsPath, true);
                Directory.CreateDirectory(installBinsPath);

                //clear old bins
                clearOldPluginBins(Path.Combine(pluginRootPath, "bins"));

                // download plugin binary
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingMiner, 0));
                var urls = EthlargementIntegratedPlugin.Instance.GetMinerBinsUrlsForPlugin().ToList();
                var downloadMinerBinsOK = await DownloadEthlargementInternalBins(urls, downloadMinerProgressChangedEventHandler, stop);
                if (!downloadMinerBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingMiner;
                    return finalState;
                }
                EthlargementIntegratedPlugin.Instance.InitAndCheckSupportedDevices(AvailableDevices.Devices.Select(dev => dev.BaseDevice));
                finalState = PluginInstallProgressState.Success;
                // cross reference local and online list
                CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: {e.Message}");
                //downloadAndInstallUpdate();
                finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedUnknown;
            }
            finally
            {
                progress?.Report(Tuple.Create(finalState, 0));
            }
            return finalState;
        }


        private static void clearOldPluginBins(string pluginBinsPath)
        {
            try
            {
                //keep only 3 versions
                var numOfKeepVersions = 3;
                var installedVersions = new DirectoryInfo(pluginBinsPath).GetDirectories("*", SearchOption.AllDirectories).ToList();
                if (installedVersions.Count() > numOfKeepVersions)
                {

                    //parse versions
                    var versionDic = new Dictionary<Version, string>();
                    foreach (var dir in installedVersions)
                    {
                        var dirName = dir.Name;
                        var version = Version.Parse(dirName);
                        versionDic.Add(version, dir.FullName);
                    }

                    //get old versions
                    var dirsToDelete = new List<string>();
                    int counter = 0;
                    foreach (var nek in versionDic.OrderByDescending(key => key.Value))
                    {
                        counter++;
                        if (counter > numOfKeepVersions)
                        {
                            dirsToDelete.Add(nek.Value);
                        }
                    }

                    foreach (var path in dirsToDelete)
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error("MinerPluginsManager", $"Clearing of old plugin bins failed: {ex.Message}");
            }            
        }
#endregion DownloadingInstalling
    }
}
