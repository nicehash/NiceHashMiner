using BrokenMiner;
using Example;
using FakePlugin;
using LolMiner;
using NanoMiner;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Configs;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginLoader;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
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
                // __DEV__*          
#if INTEGRATE_BrokenMiner_PLUGIN
                new BrokenMiner.BrokenMinerPlugin(),
#endif
#if INTEGRATE_ExamplePlugin_PLUGIN
                new ExampleMiner.ExamplePlugin(),
#endif
#if INTEGRATE_FakePlugin_PLUGIN
                new FakePlugin.FakePlugin(),
#endif

// real miners
#if INTEGRATE_NanoMiner_PLUGIN
                new NanoMinerPlugin(),
#endif
#if INTEGRATE_LolMiner_PLUGIN
                new LolMinerPlugin(),
#endif

#if INTEGRATE_ALL_PLUGINS
                new NanoMinerPlugin(),
                new LolMinerPlugin(),
#endif

            };

#if INTEGRATE_Joker_PLUGIN
            var (user_plugins, _) = InternalConfigs.GetDefaultOrFileSettings(Paths.InternalsPath("UserMinerPlugins.json"), new List<string>());
            _integratedPlugins.AddRange(user_plugins.Select(name => new MP.Joker.JokerPlugin(name)));
#endif

            (_initOnlinePlugins, OnlinePlugins) = ReadCachedOnlinePlugins();
        }

        private static readonly bool _initOnlinePlugins;

        // API data
        private static List<PluginPackageInfo> OnlinePlugins { get; set; }
        private static Dictionary<string, PluginPackageInfoCR> PluginsPackagesInfosCRs { get; set; } = new Dictionary<string, PluginPackageInfoCR>();
        private static Dictionary<string, PluginPackageInfoCR> PluginsPackagesInternal { get; set; } = new Dictionary<string, PluginPackageInfoCR>();

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
                    .ThenByDescending(info => info.SupportedDeviceCount)
                    .ThenBy(info => info.PluginName);
            }
        }
        public static IEnumerable<PluginPackageInfoCR> RankedUserPlugins
        {
            get
            {
                return PluginsPackagesInternal
                    .Select(kvp => kvp.Value)
                    .OrderBy(info => info.PluginName);
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
                var filteredIntegratedPlugins = _integratedPlugins
                    .Where(p => BlacklistedPlugins.IsNotBlacklisted(p.PluginUUID))
                    .Select(PluginContainer.Create);

                // TODO ADD STEP AND MESSAGE
                CheckAndDeleteUnsupportedPlugins();

                // load dll's and create plugin containers
                var externalLoadedPlugins = MinerPluginHost.LoadPlugins(Paths.MinerPluginsPath(new string[] { }))
                    .Where(BlacklistedPlugins.IsNotBlacklisted)
                    .Where(MinerPluginHost.MinerPlugin.ContainsKey)
                    .Select(pluginUUID => MinerPluginHost.MinerPlugin[pluginUUID])
                    .Select(PluginContainer.Create);

                var loadedPlugins = new List<PluginContainer>();
                loadedPlugins.AddRange(filteredIntegratedPlugins);
                loadedPlugins.AddRange(externalLoadedPlugins);
                // init all containers
                foreach (var plugin in loadedPlugins)
                {
                    if (!plugin.IsInitialized)
                    {
                        plugin.InitPluginContainer();
                    }

                    if (plugin.IsCompatibleInitializedAndNotBroken)
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
            RunninLoops = Task.Run(() =>
            {
                var loop1 = PluginsUpdaterLoop(stop);
                var loop2 = PluginInstaller.RestartDevicesStateLoop(stop);
                return Task.WhenAll(loop1, loop2);
            });
        }

        private static bool CanUpdatePlugin(PluginPackageInfoCR pcr) =>
            AcceptedPlugins.IsAccepted(pcr.PluginUUID)
            && pcr.IsAutoUpdateEnabled
            && pcr.HasNewerVersion 
            && pcr.CompatibleNHPluginVersion
            && pcr.HasSupportedDevices;

        private static async Task PluginsUpdaterLoop(CancellationToken stop)
        {
            try
            {
                bool isActive() => !stop.IsCancellationRequested;

                var getOnlineMinerPluginsElapsedTimeChecker = new ElapsedTimeChecker(MinerPluginsUpdaterSettings.CheckPluginsInterval, false);
                async Task<bool> updateOnlineMinerPluginsList() {
                    var check = getOnlineMinerPluginsElapsedTimeChecker.CheckAndMarkElapsedTime();
                    if (!check) return false;
                    Logger.Debug("MinerPluginsManager", $"Checking for plugin updates");
                    var success = await GetOnlineMinerPlugins();
                    if (success) CrossReferenceInstalledWithOnline();
                    var logValue = success ? "SUCCESS" : "FAIL";
                    Logger.Debug("MinerPluginsManager", $"Checking for plugin updates returned {logValue}");
                    return success;
                };

                void updateMinerPlugins() {
                    Logger.Debug("MinerPluginsManager", $"Checking plugins to Install/Update");
                    var pluginsThatCanAutoUpdate = PluginsPackagesInfosCRs.Values
                        .Where(p => p.Installed)
                        .Where(CanUpdatePlugin)
                        .Where(p => MinerPluginInstallTasks.ContainsKey(p.PluginUUID) == false) // skip if update is already in progress
                        .Select(p => p.PluginUUID)
                        .ToArray();
                    foreach (var pluginUUID in pluginsThatCanAutoUpdate)
                    {
                        Logger.Debug("MinerPluginsManager", $"Main loop Install/Update {pluginUUID}");
                        _ = _minerPluginInstallTasksProgress.TryGetValue(pluginUUID, out var progress);
                        _ = DownloadAndInstall(pluginUUID, progress, stop);
                    }
                };

                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                Logger.Debug("MinerPluginsManager", $"STARTING MAIN LOOP");
                while (isActive())
                {
                    try
                    {
                        if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);
                        var isPluginsListUpdated = await updateOnlineMinerPluginsList();
                        var executeMinerPluginsUpdate = UpdateSettings.Instance.AutoUpdateMinerPlugins && isPluginsListUpdated;
                        if (isActive() && executeMinerPluginsUpdate) updateMinerPlugins();
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
                foreach (var installTask in MinerPluginInstallTasks.Values)
                {
                    installTask.TryCancelInstall();
                }
            }
        }

        private static class PluginInstaller
        {
            private static readonly TrivialChannel<PluginInstallerCommand> Channel = new TrivialChannel<PluginInstallerCommand>();
            private abstract record PluginInstallerCommand(string PluginUUID);
            private record RemoveCommand(string PluginUUID) : PluginInstallerCommand(PluginUUID);
            private record RemovedCommand(string PluginUUID, bool Success) : PluginInstallerCommand(PluginUUID);
            private record InstallCommand(string PluginUUID) : PluginInstallerCommand(PluginUUID);
            private record InstalledCommand(string PluginUUID, bool Success) : PluginInstallerCommand(PluginUUID);

            private static bool IsRemovalCommand(PluginInstallerCommand c) => c is RemoveCommand or RemovedCommand;
            private static bool IsInstallationCommand(PluginInstallerCommand c) => c is InstallCommand or InstalledCommand;

            public static void RemovePlugin(string pluginUUID) => Channel.Enqueue(new RemoveCommand(pluginUUID));

            public static void RemovedPluginStatus(string pluginUUID, bool success) => Channel.Enqueue(new RemovedCommand(pluginUUID, success));
            

            public static void InstallPlugin(string pluginUUID) => Channel.Enqueue(new InstallCommand(pluginUUID));

            public static void InstalledPluginStatus(string pluginUUID, bool success) => Channel.Enqueue(new InstalledCommand(pluginUUID, success));

            public static async Task RestartDevicesStateLoop(CancellationToken stop)
            {
                var lastCommandTime = DateTime.UtcNow;
                bool checkCommandsForRestart() => (DateTime.UtcNow - lastCommandTime).TotalSeconds >= 0.5;
                var pairedCommands = new Dictionary<string, List<PluginInstallerCommand>>();
                var pluginsToDelete = new List<string>();
                try
                {
                    var checkWaitTime = TimeSpan.FromMilliseconds(50);
                    bool isActive() => !stop.IsCancellationRequested;
                    Logger.Info("PluginInstaller", "Starting RestartDevicesStateLoop");
                    while (isActive())
                    {
                        if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);

                        // TODO check last command time and after a delay execute device restart
                        if (pairedCommands.Any() && checkCommandsForRestart())
                        {
                            var partitionedCommands = pairedCommands.Keys
                                .ToArray()
                                .Select(pluginUUID => (pluginUUID, commands: pairedCommands[pluginUUID]))
                                .Select(p => (p.pluginUUID, removal: p.commands.Where(IsRemovalCommand), installation: p.commands.Where(IsInstallationCommand)))
                                .ToArray();

                            var currentPluginsToDelete = partitionedCommands.Where(p => p.removal.Count() > 1)
                                .Where(p => p.installation.Count() == 0)
                                .Select(p => p.pluginUUID)
                                .ToArray();
                            pluginsToDelete.AddRange(currentPluginsToDelete);

                            foreach (var (pluginUUID, removal, installation) in partitionedCommands)
                            {
                                var installAndRemoveAtSameTime = removal.Any() && installation.Any();
                                var moreThanOneCommand = removal.Count() > 1 || installation.Count() > 1;
                                var removeCommand = installAndRemoveAtSameTime || moreThanOneCommand;
                                if (installAndRemoveAtSameTime)
                                    Logger.Error("PluginInstaller", $"Plugin {pluginUUID} has installation and removal commands at same time!!!");
                                if (removeCommand) pairedCommands.Remove(pluginUUID);
                            }
                            // when we have no commands pending restart devices
                            if (!pairedCommands.Any())
                            {
                                await ApplicationStateManager.RestartDevicesState();
                                var deletePluginTasks = pluginsToDelete
                                    .Distinct()
                                    .Select(DelayedPluginDelete)
                                    .ToArray();
                                pluginsToDelete.Clear();
                                _ = Task.WhenAll(deletePluginTasks); // TODO await or leave
                            }
                        }

                        // command handling
                        var (command, hasTimedout, exceptionString) = await Channel.ReadAsync(checkWaitTime, stop);
                        if (exceptionString != null) Logger.Error("PluginInstaller", $"Channel.ReadAsync error: {exceptionString}");
                        if (command == null) continue;
                        // handle commands
                        if (!pairedCommands.ContainsKey(command.PluginUUID)) pairedCommands[command.PluginUUID] = new List<PluginInstallerCommand>() { };
                        pairedCommands[command.PluginUUID].Add(command);
                        lastCommandTime = DateTime.UtcNow;
                    }
                }
                catch (TaskCanceledException e)
                {
                    Logger.Debug("PluginInstaller", $"RestartDevicesStateLoop TaskCanceledException: {e.Message}");
                }
                finally
                {
                    Logger.Info("PluginInstaller", "Exiting RestartDevicesStateLoop run cleanup");
                    // cleanup
                }
            }
        }

        public static async Task DevicesCrossReferenceIDsWithMinerIndexes(IStartupLoader loader)
        {
            // get devices
            var baseDevices = AvailableDevices.Devices.Select(dev => dev.BaseDevice);
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatibleInitializedAndNotBroken)
                .Where(p => p.HasDevicesCrossReference())
                //.Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID)) // WARNING We still want to mine with these 
                .ToArray();

            if (checkPlugins.Length > 0 && loader != null)
            {
                loader.SecondaryVisible = true;
                loader.SecondaryTitle = Translations.Tr("Devices Cross Reference");
                loader?.SecondaryProgress?.Report((Translations.Tr("Pending"), 0));
            }
            var pluginDoneCount = 0d;
            foreach (var plugin in checkPlugins)
            {
                try
                {
                    Logger.Info("MinerPluginsManager", $"Cross Reference {plugin.Name}_{plugin.Version}_{plugin.PluginUUID}");
                    loader?.SecondaryProgress?.Report((Translations.Tr("Cross Reference {0}", plugin.Name), (int)((pluginDoneCount / checkPlugins.Length) * 100)));
                    await plugin.DevicesCrossReference(baseDevices);
                    pluginDoneCount += 1;
                    loader?.SecondaryProgress?.Report((Translations.Tr("Cross Reference {0}", plugin.Name), (int)((pluginDoneCount / checkPlugins.Length) * 100)));
                }
                catch (Exception e)
                {
                    Logger.Error("MinerPluginsManager", $"DevicesCrossReferenceIDsWithMinerIndexes error: {e.Message}");
                }
            }
            if (loader != null)
            {
                loader.SecondaryVisible = false;
            }
        }

        public static async Task DownloadMissingMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            var pluginsWithMissingPackageFiles = PluginContainer.PluginContainers
                .Where(p => p.IsCompatibleInitializedAndNotBroken)
                .Where(p => p.HasMisingBinaryPackageFiles())
                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                .Where(p => !BlacklistedPlugins.IsDownloadPermaBan(p.PluginUUID))
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

        private static string PluginInstallProgressStateToString(PluginInstallProgressState state, string pluginName, int progressPerc)
        {
            return state switch
            {
                PluginInstallProgressState.DownloadingMiner => Translations.Tr("Downloading Miner: {0}%", $"{pluginName} {progressPerc}"),
                PluginInstallProgressState.DownloadingPlugin => Translations.Tr("Downloading Plugin: {0}%", $"{pluginName} {progressPerc}"),
                PluginInstallProgressState.ExtractingMiner => Translations.Tr("Extracting Miner: {0}%", $"{pluginName} {progressPerc}"),
                PluginInstallProgressState.ExtractingPlugin => Translations.Tr("Extracting Plugin: {0}%", $"{pluginName} {progressPerc}"),
                _ => Translations.Tr("Pending Install") + $" {pluginName}",
            };
        }

        public static async Task UpdateMinersBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            bool hasUpdate(PluginContainer p) =>
                PluginsPackagesInfosCRs.TryGetValue(p.PluginUUID, out var pcr) && CanUpdatePlugin(pcr);

            var pluginsToUpdate = PluginContainer.PluginContainers
                .Where(p => p.IsCompatibleInitializedAndNotBroken)
                .Where(hasUpdate)
                .ToArray();

            foreach (var plugin in pluginsToUpdate)
            {
                var wrappedProgress = new Progress<Tuple<PluginInstallProgressState, int>>(status =>
                {
                    var (state, progressPerc) = status;
                    string statusText = PluginInstallProgressStateToString(state, plugin.Name, progressPerc);
                    progress?.Report((statusText, progressPerc));
                });
                await DownloadAndInstall(plugin.PluginUUID, wrappedProgress, stop);
            }
        }

        internal static bool CanFallbackAndMineWithPlugin(IMinerPlugin plugin)
        {
            try
            {
                // call to check if it throws and exception
                var uuid = plugin.PluginUUID;
                var version = plugin.Version;
                // finally we must guarantee that we don't have any missing miner files
                return plugin is IBinaryPackageMissingFilesChecker impl && impl.CheckBinaryPackageMissingFiles().Count() == 0;
            }
            catch (Exception)
            {
                // for what ever reason skip this plugin
                return false;
            }
        }

        public static List<PluginPackageInfoCR> PluginsForEulaConfirm { get; private set; } = new List<PluginPackageInfoCR>();

        private static void CheckAccepted3rdPartyPlugins()
        {
            var nonAcceptedlugins = PluginContainer.PluginContainers
                .Where(p => p.IsCompatibleInitializedAndNotBroken)
                .Where(p => !AcceptedPlugins.IsAccepted(p.PluginUUID))
                .ToArray();
            var nonAcceptedluginsUUIDs = nonAcceptedlugins
                .Select(p => p.PluginUUID)
                .ToArray();
            PluginsForEulaConfirm = RankedPlugins.Where(pcr => nonAcceptedluginsUUIDs.Contains(pcr.PluginUUID)).ToList();
            PluginsForEulaConfirm.ForEach(el =>
            {
                Logger.Info("MinerPluginsManager", $"Plugin EULA is not accepted {el.PluginUUID}-{el.PluginName}. Skipping...");
                el.IsUserActionRequired = true;
            });
            // we check this in order to keep mining in case of an update from NHM version without 3rd party EULA
            var nonAcceptedPluginsWithMissingBinaries = nonAcceptedlugins
                .Where(p => p.HasMisingBinaryPackageFiles())
                .Where(p => Directory.Exists(Paths.MinerPluginsPath(p.PluginUUID, "dlls")))
                .ToArray();
            foreach (var p in nonAcceptedPluginsWithMissingBinaries)
            {
                try
                {
                    var oldPlugins = Directory.GetFiles(Paths.MinerPluginsPath(p.PluginUUID, "dlls"), "*.dll", SearchOption.AllDirectories)
                        .SelectMany(MinerPluginHost.LoadPluginsFromDllFile)
                        .Where(CanFallbackAndMineWithPlugin)
                        .Where(plugin => plugin.PluginUUID == p.PluginUUID)
                        .OrderByDescending(plugin => plugin.Version)
                        .ToList();
                    if (oldPlugins.Any()) PluginContainer.RemovePluginContainer(p);
                    foreach (var fallbackPlugin in oldPlugins)
                    {
                        // init all containers
                        var plugin = PluginContainer.Create(fallbackPlugin);
                        if (!plugin.IsInitialized)
                        {
                            plugin.InitPluginContainer();
                        }

                        if (plugin.IsCompatibleInitializedAndNotBroken)
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

        private static async Task DelayedPluginDelete(string pluginUUID)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var deletePath = Paths.MinerPluginsPath(pluginUUID);
            var start = DateTime.UtcNow;
            while (true)
            {
                var elapsedAfterStart = DateTime.UtcNow - start;
                if (elapsedAfterStart.TotalSeconds > 15) return;
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                try
                {
                    if (Directory.Exists(deletePath))
                    {
                        Directory.Delete(deletePath, true);
                        return;
                    }
                }
                catch { }
            }
        }

        public static async Task RemovePlugin(string pluginUUID, bool crossReferenceInstalledWithOnline = true)
        {
            BlacklistedPlugins.AddToBlacklist(pluginUUID);
            try
            {
                PluginInstaller.RemovePlugin(pluginUUID);

                AcceptedPlugins.Remove(pluginUUID);
                MinerPluginHost.MinerPlugin.Remove(pluginUUID);
                var oldPlugins = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUUID).ToArray();
                foreach (var old in oldPlugins) PluginContainer.RemovePluginContainer(old);

                // remove from cross ref dict
                if (PluginsPackagesInfosCRs.ContainsKey(pluginUUID))
                {
                    PluginsPackagesInfosCRs[pluginUUID].LocalInfo = null;
                    // we might not have any online reference so remove it in this case
                    if (PluginsPackagesInfosCRs[pluginUUID].OnlineInfo == null)
                    {
                        PluginsPackagesInfosCRs.Remove(pluginUUID);
                    }
                }

                if (crossReferenceInstalledWithOnline) CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Error occured while removing {pluginUUID} plugin: {e.Message}");
            }
            finally
            {
                PluginInstaller.RemovedPluginStatus(pluginUUID, true);
#if NHMWS4
                ApplicationStateManager.ReSendLoginMessage();
#endif
                await Task.CompletedTask;
            }
        }


        private static string ConstructLocalPluginDescription(PluginBase plugin)
        {
            var binVersion = plugin.GetMinerBinaryVersion();
            return $"Miner Binary Version '{binVersion}'.\n\n" + plugin.GetPluginMetaInfo().PluginDescription;
        }

        private static int GetPluginDeviceRank(PluginPackageInfo info)
        {
            if (info.SupportedDevicesAlgorithms == null) return 0;
            var supportedDevices = info.SupportedDevicesAlgorithms
                .Where(kvp => kvp.Value.Count > 0)
                .Select(kvp => kvp.Key);
            var devRank = AvailableDevices.Devices
                .Where(d => supportedDevices.Contains($"{d.DeviceType}"))
                .Count();
            return devRank;
        }
        public static void CrossReferenceInstalledWithOnline()
        {
            // first go over the installed plugins
            var installedPlugins = PluginContainer.PluginContainers
                //.Where(p => p.Enabled) // we can have installed plugins that are obsolete
                .Where(p => !_integratedPlugins.Any(integrated => integrated.PluginUUID == p.PluginUUID)) // ignore integrated
                .ToArray();
            //integrated zone TODO MAKE FUNCTION
            var integratedPlugins = PluginContainer.PluginContainers
                .Where(p => _integratedPlugins.Any(integrated => integrated.PluginUUID == p.PluginUUID))?
                .ToArray();
            foreach(var integrated in integratedPlugins)
            {
                var (uuid, localPluginInfo) = CreateLocalPackageInfo(integrated);
                if (PluginsPackagesInternal.ContainsKey(uuid) == false)
                {
                    PluginsPackagesInternal[uuid] = new PluginPackageInfoCR(uuid);
                }
                PluginsPackagesInternal[uuid].LocalInfo = localPluginInfo;
            }
            //end integrated zone
            foreach (var installed in installedPlugins)
            {
                var (uuid, localPluginInfo) = CreateLocalPackageInfo(installed);
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
            }
            foreach (var plugin in PluginsPackagesInfosCRs)
            {
                PluginsPackagesInfosCRs[plugin.Key].SupportedDeviceCount = GetPluginDeviceRank(plugin.Value.GetInfoSource());
            }
            MinerPluginsManagerState.Instance.UserPlugins = RankedUserPlugins.ToList();
            MinerPluginsManagerState.Instance.RankedPlugins = RankedPlugins.ToList();
        }
        private static (string uuid, PluginPackageInfo packageInfo) CreateLocalPackageInfo(PluginContainer pluginContainer)
        {
            var uuid = pluginContainer.PluginUUID;
            var localPluginInfo = new PluginPackageInfo
            {
                PluginAuthor = pluginContainer.Author,
                PluginName = pluginContainer.Name,
                PluginUUID = uuid,
                PluginVersion = pluginContainer.Version,
                // other stuff is not inside the plugin
            };
            if (pluginContainer.GetPlugin() is PluginBase pb)
            {
                localPluginInfo.MinerPackageURL = pb.GetMinerBinsUrlsForPlugin().FirstOrDefault();
                localPluginInfo.PluginDescription = ConstructLocalPluginDescription(pb);
                localPluginInfo.SupportedDevicesAlgorithms = new Dictionary<string, List<string>>();
                localPluginInfo.PackagePassword = pb.BinsPackagePassword;
                var supportedList = pb.SupportedDevicesAlgorithmsDict();
                foreach (var supported in supportedList)
                {
                    var algos = supported.Value.Select(algo => Enum.GetName(typeof(AlgorithmType), algo)).ToList();
                    localPluginInfo.SupportedDevicesAlgorithms.Add(Enum.GetName(typeof(DeviceType), supported.Key), algos);
                }
            }
            return (uuid,localPluginInfo);
        }

        private static async Task<List<PluginPackageInfo>?> FetchPluginsOnline(int version)
        {
            using var client = new NoKeepAliveHttpClient();
            string s = await client.GetStringAsync($"{Links.PluginsJsonApiUrl}?v={version}");
            PluginsListCacheManager.WritePluginCache(version, s);
            return JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
            });
        } 

        private static async Task<bool> GetOnlineMinerPlugins()
        {
            async Task<List<PluginPackageInfo>> getPlugins(int version)
            {
                var shouldUpdate = await PluginsListCacheManager.CheckIfShouldUpdateAndUpdateLatestDate(Links.PluginsJsonApiUrl, version);
                Logger.Info("MinerPluginsManager", $"Should update plugins ({version}) {shouldUpdate}");
                if (shouldUpdate)
                {
                    return await FetchPluginsOnline(version);
                }
                var readPluginList = PluginsListCacheManager.ReadPluginCache(version);
                if(readPluginList == string.Empty)
                {
                    Logger.Warn("MinerPluginsManager", $"ReadPluginList was empty");
                    return await FetchPluginsOnline(version);
                }
                try
                {
                    var deserialized = JsonConvert.DeserializeObject<List<PluginPackageInfo>>(readPluginList, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Culture = CultureInfo.InvariantCulture
                    });
                    return deserialized;
                }
                catch (Exception ex)
                {
                    Logger.Error("MinerPluginsManager", $"Error occured while deserializing cached plugins: {ex.Message}");
                    return await FetchPluginsOnline(version);
                }
            }
            try
            {
                var random = new Random();
                var onlinePluginsAllVersions = new List<PluginPackageInfo>();
                foreach (var version in Checkers.SupportedMajorVersions)
                {
                    onlinePluginsAllVersions.AddRange(await getPlugins(version));
                    await Task.Delay(TimeSpan.FromMilliseconds(100 + random.Next(0, 200)));
                }
                OnlinePlugins = onlinePluginsAllVersions.GroupBy(p => p.PluginUUID)
                                        .Select(g => g.OrderByDescending(p => p.PluginVersion).FirstOrDefault())
                                        .Where(p => p != null)
                                        .ToList();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Error occured while getting online miner plugins: {e.Message}");
            }
            return _initOnlinePlugins;
        }

        private static (bool, List<PluginPackageInfo>) ReadCachedOnlinePlugins()
        {
            try
            {
                var cachedPluginsInfoPath = Paths.RootPath("plugins_packages", "update.json");
                if (!File.Exists(cachedPluginsInfoPath)) return (false, null);
                string s = File.ReadAllText(cachedPluginsInfoPath);

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
            var pluginUUID = pluginContainer.PluginUUID;
            var ver = pluginContainer.Version;
            var installingPluginBinsPath = Paths.MinerPluginsPath(pluginUUID, "bins", $"{ver.Major}.{ver.Minor}");
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
                clearOldPluginBins(Paths.MinerPluginsPath(pluginUUID, "bins"));
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsManager", $"Installation of {pluginUUID} failed: ${e.Message}");
            }
        }

        private static ConcurrentDictionary<string, MinerPluginInstallTask> MinerPluginInstallTasks = new ConcurrentDictionary<string, MinerPluginInstallTask>();
        // with WPF we have only one Progress 
        private static ConcurrentDictionary<string, IProgress<Tuple<PluginInstallProgressState, int>>> _minerPluginInstallTasksProgress = new ConcurrentDictionary<string, IProgress<Tuple<PluginInstallProgressState, int>>>();

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

        public static async Task DownloadAndInstall(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress, CancellationToken stop)
        {
            if (MinerPluginInstallTasks.ContainsKey(pluginUUID)) return;

            var addSuccess = false;
            var installSuccess = false;
            var installResult = PluginInstallProgressState.Canceled;
            using var minerInstall = new MinerPluginInstallTask();
            using var tcs = CancellationTokenSource.CreateLinkedTokenSource(stop, minerInstall.CancelInstallToken);
            try
            {
                PluginInstaller.InstallPlugin(pluginUUID);
                var pluginPackageInfo = PluginsPackagesInfosCRs[pluginUUID];
                addSuccess = MinerPluginInstallTasks.TryAdd(pluginUUID, minerInstall);
                if (progress != null)
                {
                    progress?.Report(Tuple.Create(PluginInstallProgressState.Pending, 0));
                    minerInstall.AddProgress(progress);
                }
                installResult = await DownloadAndInstall(pluginPackageInfo, minerInstall, tcs.Token);
                installSuccess = installResult == PluginInstallProgressState.Success;
            }
            finally
            {
                Logger.Info("MinerPluginsManager", $"DownloadAndInstall {pluginUUID} result: {installResult}");
                PluginInstaller.InstalledPluginStatus(pluginUUID, installSuccess);
                MinerPluginInstallTasks.TryRemove(pluginUUID, out var _);
                if (addSuccess) BlacklistedPlugins.RemoveFromBlacklist(pluginUUID);


                AvailableNotifications.CreatePluginUpdateInfo(PluginsPackagesInfosCRs[pluginUUID].PluginName, installSuccess);
#if NHMWS4
                ApplicationStateManager.ReSendLoginMessage();
#endif
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
                var pluginRootPath = Paths.MinerPluginsPath(plugin.PluginUUID);
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
                if (!FilePathHashEqualsToDatabaseHash(downloadPluginResult.downloadedFilePath, plugin.PluginPackageHash, plugin.PluginName))
                {
                    //uninstall plugin dll
                    if (File.Exists(downloadPluginResult.downloadedFilePath))
                    {
                        File.Delete(downloadPluginResult.downloadedFilePath);
                    }
                    AvailableNotifications.CreateFailedDownloadWrongHashBinary(plugin.PluginName);
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedWrongHashPlugin;
                    return finalState;
                }
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
                if (!FilePathHashEqualsToDatabaseHash(downloadMinerBinsResult.downloadedFilePath, plugin.BinaryPackageHash, plugin.PluginName))
                {
                    //uninstall plugin binary
                    if (File.Exists(downloadMinerBinsResult.downloadedFilePath))
                    {
                        File.Delete(downloadMinerBinsResult.downloadedFilePath);
                    }
                    AvailableNotifications.CreateFailedDownloadWrongHashBinary(plugin.PluginName);
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedWrongHashMiner;
                    return finalState;
                }
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
            catch (Exception ex)
            {
                Logger.Error("MinerPluginsManager", $"Clearing of old plugin bins failed: {ex.Message}");
            }
        }
        #endregion DownloadingInstalling

        private static bool FilePathHashEqualsToDatabaseHash(string filepath, string databaseHash, string pluginName)
        {
            const string EMPTY_HASH = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            string hashString = FileHelpers.GetFileSHA256Checksum(filepath);
            if (hashString == EMPTY_HASH)
            {
                AvailableNotifications.CreateNullChecksumError(pluginName);
                Logger.Error("MinerPluginsManager", "Downloaded file is empty");
            }
            return hashString == databaseHash.ToLowerInvariant();
        }
    }
}
