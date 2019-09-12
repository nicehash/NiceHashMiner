using NHM.Common;
using NHM.MinersDownloader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class UpdateHelpers
    {
        public static async Task DownloadUpdaterAsync(Progress<int> downloadProgress)
        {
            try
            {
                var url = ApplicationStateManager.GetNewVersionUpdaterUrl();
                var downloadRootPath = Path.Combine(Paths.Root, "updaters");
                if (!Directory.Exists(downloadRootPath))
                {
                    Directory.CreateDirectory(downloadRootPath);
                }
                var (success, downloadedFilePath) = await MinersDownloadManager.DownloadFileWebClientAsync(url, downloadRootPath, $"nhm_windows_updater_{ApplicationStateManager.OnlineVersion}", downloadProgress, ApplicationStateManager.ExitApplication.Token);
                if (!success || ApplicationStateManager.ExitApplication.Token.IsCancellationRequested) return;

                // stop devices
                ApplicationStateManager.StopAllDevice();

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
    }
}
