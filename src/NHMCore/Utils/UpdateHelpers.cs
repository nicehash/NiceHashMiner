using Microsoft.Win32;
using NHM.Common;
using NHM.MinersDownloader;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class UpdateHelpers
    {
        // TODO null for now
        private static Progress<int> DownloadProgress { get; set; } = null;
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
            RunninLoops = Task.Run(() => {
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
                    if (isActive() && isAutoUpdate && hasNewVersion && !Launcher.IsUpdatedFailed)
                    {
                        try
                        {
                            // determine what how to update
                            bool isUpdater = IsNHMInstalled() && IsRunningInstalledApp();
                            // #1 download updater.exe or zip depending on bin type
                            var url = isUpdater ? VersionState.Instance.GetNewVersionUpdaterUrl() : VersionState.Instance.GetNewVersionZipUrl();
                            var downloadRootPath = Path.Combine(Paths.Root, "updaters");
                            if (!Directory.Exists(downloadRootPath))
                            {
                                Directory.CreateDirectory(downloadRootPath);
                            }
                            var saveAsFile = isUpdater ? $"nhm_windows_updater_{VersionState.Instance.OnlineVersionStr}" : $"nhm_windows_{VersionState.Instance.OnlineVersionStr}";
                            var (success, downloadedFilePath) = await MinersDownloadManager.DownloadFileWebClientAsync(url, downloadRootPath, saveAsFile, DownloadProgress, ApplicationStateManager.ExitApplication.Token);
                            if (!success)
                            {
                                // TODO notify that we cannot download the miner updates file
                                continue;
                            }

                            OnAutoUpdate?.Invoke();
                            // #2 SAVE current state so we can resume it after the client updates
                            ApplicationStateManager.SaveMiningState();
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
                                // TODO non launcher not priority right now
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(Tag, $"Check autoupdate Exception: {ex.Message}");
                        }
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



        public static async Task DownloadUpdaterAsync(Progress<int> downloadProgress)
        {
            try
            {
                var url = VersionState.Instance.GetNewVersionUpdaterUrl();
                var downloadRootPath = Path.Combine(Paths.Root, "updaters");
                if (!Directory.Exists(downloadRootPath))
                {
                    Directory.CreateDirectory(downloadRootPath);
                }
                var (success, downloadedFilePath) = await MinersDownloadManager.DownloadFileWebClientAsync(url, downloadRootPath, $"nhm_windows_updater_{VersionState.Instance.OnlineVersionStr}", downloadProgress, ApplicationStateManager.ExitApplication.Token);
                if (!success || ApplicationStateManager.ExitApplication.Token.IsCancellationRequested) return;

                // stop devices
                await ApplicationStateManager.StopAllDevicesTask();

                using (var updater = new Process())
                {
                    updater.StartInfo.UseShellExecute = false;
                    updater.StartInfo.FileName = downloadedFilePath;
                    updater.Start();
                }
            } catch (Exception ex)
            {
                Logger.Error("UpdateHelpers", $"Updating failed: {ex.Message}");
            }
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
            var localAppData =  Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return path.Contains(localAppData);
        }
    }
}
