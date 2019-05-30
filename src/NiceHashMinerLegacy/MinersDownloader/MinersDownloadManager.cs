using MyDownloader.Core;
using MyDownloader.Core.Extensions;
using MyDownloader.Core.UI;
using MyDownloader.Extension.Protocols;
using Newtonsoft.Json;
using NiceHashMiner.Utils;
using NiceHashMinerLegacy.Common;
using SharpCompress.Archives.SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NiceHashMiner.MinersDownloader
{
    public static class MinersDownloadManager
    {
        #region DownloadSetup - Internal settings 
        internal class DownloadSetup
        {
            public DownloadSetup(string url, string dlName)
            {
                BinsDownloadUrl = url;
                BinsZipLocation = dlName;
            }

            public string BinsDownloadUrl { get; set; }
            public string BinsZipLocation { get; set; }
        }
        private static readonly DownloadSetup StandardDlSetup = new DownloadSetup(
            "https://github.com/nicehash/NiceHashMinerTest/releases/download/1.9.1.4/bin.7z",
            "bins.7z");

        private static readonly DownloadSetup ThirdPartyDlSetup = new DownloadSetup(
            "https://github.com/nicehash/NiceHashMinerTest/releases/download/1.9.1.4/bin_3rdparty.7z",
            "bins_3rdparty.7z");

        static MinersDownloadManager()
        {
            const string binsUrlSettings = "bins_urls.json";
            if (File.Exists(binsUrlSettings))
            {
                var downloadSettings = JsonConvert.DeserializeObject<List<DownloadSetup>>(File.ReadAllText(binsUrlSettings), Globals.JsonSettings);
                if (downloadSettings != null)
                {
                    if (downloadSettings.Count >= 1)
                    {
                        StandardDlSetup = downloadSettings[0];
                    }
                    if (downloadSettings.Count >= 2)
                    {
                        ThirdPartyDlSetup = downloadSettings[1];
                    }
                }
            }
        }
        #endregion DownloadSetup - Internal settings 

        public static Task DownloadAndExtractOpenSourceMinersWithMyDownloaderAsync(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            return DownloadAndExtractWithMyDownloaderAsync(StandardDlSetup, progress, stop);
        }

        public static Task DownloadAndExtractThirdPartyMinersWithMyDownloaderAsync(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            return DownloadAndExtractWithMyDownloaderAsync(ThirdPartyDlSetup, progress, stop);
        }

        private static async Task DownloadAndExtractWithMyDownloaderAsync(DownloadSetup downloadSetup, IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            // if something not right delete previous and download new
            try
            {
                if (File.Exists(downloadSetup.BinsZipLocation))
                {
                    File.Delete(downloadSetup.BinsZipLocation);
                }
                // TODO don't delete 'miner_plugins' folder
                //if (Directory.Exists(downloadSetup.ZipedFolderName))
                //{
                //    Directory.Delete(downloadSetup.ZipedFolderName, true);
                //}
            }
            catch (Exception e)
            {
                Logger.Error("MinersDownloadManager", $"Error occured while downloading files with myDownloader: {e.Message}");
            }

            try
            {
                var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Downloading {0} %", perc), perc)));
                var result = await NHM.MinersDownloader.MinersDownloadManager.DownloadFileAsync(downloadSetup.BinsDownloadUrl, "", downloadSetup.BinsZipLocation.Split('.').First(), downloadProgress, stop);
                var downloadedSuccess = result.success;
                if (!downloadedSuccess || stop.IsCancellationRequested) {
                    Logger.Info("MinersDownloadManager", $"Download success={downloadedSuccess} || cancel={stop.IsCancellationRequested}");
                    return;
                }

                var zipFilePath = result.downloadedFilePath;
                var zipFileExists = false;
                const int maxWait = 3000;
                const int stepWait = 300;
                int waitLeft = maxWait;
                while (waitLeft > 0)
                {
                    waitLeft -= stepWait;
                    if (File.Exists(zipFilePath))
                    {
                        zipFileExists = true;
                        break;
                    }
                    await Task.Delay(stepWait);
                }
                if (!zipFileExists)
                {
                    Logger.Info("MinersDownloadManager", $"Downloaded file {zipFilePath} doesn't exist, exiting");
                    return;
                }

                var unzipProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Unzipping {0} %", perc), perc)));
                await ArchiveHelpers.ExtractFileAsync(zipFilePath, "miner_plugins", unzipProgress, stop);
                
                await Task.Delay(300);
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
            catch (Exception e)
            {
                Logger.Error("MinersDownloadManager", $"Error occured while downloading and extracting with myDownloader: {e.Message}");
            }
        }
    }
}
