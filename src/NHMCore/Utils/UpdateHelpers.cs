using Microsoft.Win32;
using NHM.Common;
using NHM.MinersDownloader;
using NHMCore.ApplicationState;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class UpdateHelpers
    {
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
            if (Registry.CurrentUser.OpenSubKey(@"Software\" + APP_GUID.GUID, false) != null)
            {
                isInstalled = true;
            }
            return isInstalled;
        }
    }
}
