using Microsoft.Win32;
using NHM.Common;
using NHM.MinersDownloader;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Notifications;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class UpdateHelpers
    {
        private static readonly string Tag = "UpdateHelpers";

        public static Task RunninLoops { get; private set; } = null;

        public static Action OnAutoUpdate;

        public static void StartLoops(CancellationToken stop)
        {
            // clear old updaters when starting this loop
            var downloadRootPath = Path.Combine(Paths.Root, "updaters");
            if (Directory.Exists(downloadRootPath))
            {
                var doFiles = Directory.GetFiles(downloadRootPath);
                foreach (var doFile in doFiles)
                {
                    try
                    {
                        File.Delete(doFile);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            RunninLoops = Task.Run(() =>
            {
                var loop1 = NhmAutoUpdateCheckLoop(stop);
                return Task.WhenAll(loop1);
            });
        }

        private static async Task NhmAutoUpdateCheckLoop(CancellationToken stop)
        {
            try
            {
                // check for updates every 30s
                //var checkWaitTime = TimeSpan.FromSeconds(30);
                var checkWaitTime = TimeSpan.FromSeconds(60); // TODO DEBUG
                Func<bool> isActive = () => !stop.IsCancellationRequested;

                while (isActive())
                {
                    if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);

                    var isAutoUpdate = UpdateSettings.Instance.AutoUpdateNiceHashMiner;
                    var hasNewVersion = VersionState.Instance.IsNewVersionAvailable;
                    // prevent sleep check

                    bool isUpdater = IsNHMInstalled() && IsRunningInstalledApp();
                    if (hasNewVersion)
                    {
                        AvailableNotifications.CreateNhmUpdateInfoDownload(isUpdater);
                    }

                    if (isActive() && isAutoUpdate && hasNewVersion && !Launcher.IsUpdatedFailed)
                    {
                        var ok = await StartAutoUpdateProcess(isUpdater);
                        if (!ok) AvailableNotifications.CreateNhmUpdateAttemptFail();
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Logger.Info(Tag, $"NhmAutoUpdateCheckLoop TaskCanceledException: {e.Message}");
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"NhmAutoUpdateCheckLoop Exception: {e.Message}");
            }
            finally
            {
                Logger.Info(Tag, "Exiting NhmAutoUpdateCheckLoop run cleanup");
                // cleanup
            }
        }

        private static async Task<bool> StartAutoUpdateProcess(bool isUpdater)
        {
            try
            {
                var downloadOk = await StartDownloadingUpdater(isUpdater);
                if (!downloadOk) return false;
                return await StartUpdateProcess();
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, $"Check autoupdate Exception: {ex.Message}");
                return false;
            }
        }

        internal static async Task<bool> StartDownloadingUpdater(bool isUpdater)
        {
            try
            {
                // Let user know that something is happening after update process started
                var updateNotification = NotificationsManager.Instance.Notifications.Find(notif => notif.Group == NotificationsGroup.NhmUpdate);
                if (updateNotification != null) updateNotification.NotificationContent = Translations.Tr("Download in progress...");

                // determine what how to update
                // #1 download updater.exe or zip depending on bin type
                var url = isUpdater ? VersionState.Instance.GetNewVersionUpdaterUrl() : VersionState.Instance.GetNewVersionZipUrl();
                var downloadRootPath = Path.Combine(Paths.Root, "updaters");
                if (!Directory.Exists(downloadRootPath))
                {
                    Directory.CreateDirectory(downloadRootPath);
                }
                var saveAsFile = isUpdater ? $"nhm_windows_updater_{VersionState.Instance.OnlineVersionStr}" : $"nhm_windows_{VersionState.Instance.OnlineVersionStr}";
                var downloadProgress = updateNotification?.Action?.Progress ?? null;
                var (success, downloadedFilePath) = await MinersDownloadManager.DownloadFileWebClientAsync(url, downloadRootPath, saveAsFile, downloadProgress, ApplicationStateManager.ExitApplication.Token);
                if (!success)
                {
                    if (updateNotification != null) updateNotification.NotificationContent = Translations.Tr("Download unsuccessfull");
                    return false;
                }
                else
                {
                    if (updateNotification != null) updateNotification.NotificationContent = Translations.Tr("Download successfull");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, $"StartDownloadingUpdater autoupdate Exception: {ex.Message}");
                return false;
            };
        }

        internal static async Task<bool> StartUpdateProcess()
        {
            try
            {
                OnAutoUpdate?.Invoke();
                // #2 SAVE current state so we can resume it after the client updates
                ApplicationStateManager.SaveMiningState();
                await Task.Delay(5000); // wait 5 seconds
                await ApplicationStateManager.StopAllDevicesTask();
                await Task.Delay(5000); // wait 5 seconds
                                        // #3 restart accordingly if launcher or self containd app
                if (Launcher.IsLauncher)
                {
                    try
                    {
                        // TODO here save what version and maybe kind of update we have
                        File.Create(Paths.RootPath("do.update"));
                        ApplicationStateManager.ExecuteApplicationExit();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("NICEHASH", $"Autoupdate IsLauncher error: {e.Message}");
                        // IF we fail restore mining state and show autoupdater failure nofitication
                        await ApplicationStateManager.RestoreMiningState();
                        // TODO notify that the auto-update wasn't successful
                    }
                }
                else
                {
                    // no launcher
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, $"StartUpdateProcess autoupdate Exception: {ex.Message}");
                return false;
            }
            // if success we close app anyways
            return true;
        }

        public static bool IsNHMInstalled()
        {
            var isInstalled = false;
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\" + APP_GUID.GUID, false))
            {
                isInstalled = key != null;
            }
            return isInstalled;
        }

        public static bool IsRunningInstalledApp()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return path.Contains(localAppData);
        }
    }
}
