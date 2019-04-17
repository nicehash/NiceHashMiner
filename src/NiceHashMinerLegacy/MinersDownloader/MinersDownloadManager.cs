using MyDownloader.Core;
using MyDownloader.Core.Extensions;
using MyDownloader.Core.UI;
using MyDownloader.Extension.Protocols;
using Newtonsoft.Json;
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
        private static readonly DownloadSetup StandardDlSetup = new DownloadSetup(
            "https://github.com/nicehash/NiceHashMinerLegacyTest/releases/download/1.9.1.4/bin.7z",
            "bins.7z");

        private static readonly DownloadSetup ThirdPartyDlSetup = new DownloadSetup(
            "https://github.com/nicehash/NiceHashMinerLegacyTest/releases/download/1.9.1.4/bin_3rdparty.7z",
            "bins_3rdparty.7z");

        static MinersDownloadManager()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

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

        public static Task DownloadAndExtractOpenSourceMinersAsync(IProgress<Tuple<ProgressState, int>> progress, CancellationToken stop)
        {
            return DownloadAndExtractAsync(StandardDlSetup, progress, stop);
        }

        public static Task DownloadAndExtractThirdPartyMinersAsync(IProgress<Tuple<ProgressState, int>> progress, CancellationToken stop)
        {
            return DownloadAndExtractAsync(ThirdPartyDlSetup, progress, stop);
        }

        private static async Task DownloadAndExtractAsync(DownloadSetup downloadSetup, IProgress<Tuple<ProgressState, int>> progress, CancellationToken stop)
        {
            var downloadMinersProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(new Tuple<ProgressState, int>(ProgressState.DownloadingMiners, perc)));
            var zipProgressMinersChangedEventHandler = new Progress<int>(perc => progress?.Report(new Tuple<ProgressState, int>(ProgressState.ExtractingMiners, perc)));

            try
            {
                var downloadPluginOK = await DownloadFileAsync(downloadSetup.BinsDownloadUrl, downloadSetup.BinsZipLocation, downloadMinersProgressChangedEventHandler, stop);
                if (!downloadPluginOK || stop.IsCancellationRequested) return;
                // unzip 
                var unzipPluginOK = false;
                if (downloadSetup.BinsZipLocation.EndsWith(".7z"))
                {
                    unzipPluginOK = await Un7zipFileAsync(downloadSetup.BinsZipLocation, "miner_plugins", zipProgressMinersChangedEventHandler, stop);
                }
                else
                {
                    unzipPluginOK = await UnzipFileAsync(downloadSetup.BinsZipLocation, "miner_plugins", zipProgressMinersChangedEventHandler, stop);
                }
                if (!unzipPluginOK || stop.IsCancellationRequested) return;
                File.Delete(downloadSetup.BinsZipLocation);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("MinersDownloadManager", $"DownloadAndExtract failed: {e}");
            }
        }

        public static async Task<bool> DownloadFileAsync(string url, string downloadFileLocation, IProgress<int> progress, CancellationToken stop)
        {
            var downloadStatus = false;
            using (var client = new WebClient())
            {
                client.Proxy = null;
                client.DownloadProgressChanged += (s, e1) => {
                    progress?.Report(e1.ProgressPercentage);
                };
                client.DownloadFileCompleted += (s, e) =>
                {
                    downloadStatus = !e.Cancelled && e.Error == null;
                };
                stop.Register(client.CancelAsync);
                // Starts the download
                await client.DownloadFileTaskAsync(new Uri(url), downloadFileLocation);
            }
            return downloadStatus;
        }

        public static async Task<bool> UnzipFileAsync(string zipLocation, string unzipLocation, IProgress<int> progress, CancellationToken stop)
        {
            try
            {
                using (var archive = ZipFile.OpenRead(zipLocation))
                {
                    float entriesCount = archive.Entries.Count;
                    float extractedEntries = 0;
                    foreach (var entry in archive.Entries)
                    {
                        if (stop.IsCancellationRequested) break;

                        extractedEntries += 1;
                        var isDirectory = entry.Name == "";
                        if (isDirectory) continue;

                        var prog = ((extractedEntries / entriesCount) * 100.0f);
                        progress?.Report((int)prog);

                        var extractPath = Path.Combine(unzipLocation, entry.FullName);
                        var dirPath = Path.GetDirectoryName(extractPath);
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                        }
                        //entry.ExtractToFile(extractPath, true);

                        using (var zipStream = entry.Open())
                        using (var fileStream = new FileStream(extractPath, FileMode.Create, FileAccess.Write))
                        {
                            await zipStream.CopyToAsync(fileStream);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                // TODO log
                return false;
            }
        }

        public static async Task<bool> Un7zipFileAsync(string fileLocation, string extractLocation, IProgress<int> progress, CancellationToken stop)
        {
            try
            {
                using (Stream stream = File.OpenRead(fileLocation))
                using (var archive = SevenZipArchive.Open(stream))
                using (var reader = archive.ExtractAllEntries())
                {
                    float extractedEntries = 0;  
                    float entriesCount = archive.Entries.Count;
                    while (reader.MoveToNextEntry())
                    {
                        extractedEntries += 1;
                        if (!reader.Entry.IsDirectory)
                        {
                            var extractPath = Path.Combine(extractLocation, reader.Entry.Key);
                            var dirPath = Path.GetDirectoryName(extractPath);
                            if (!Directory.Exists(dirPath))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                            }
                            using (var entryStream = reader.OpenEntryStream())
                            using (var fileStream = new FileStream(extractPath, FileMode.Create, FileAccess.Write))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }
                        }
                        var prog = ((extractedEntries / entriesCount) * 100.0f);
                        progress?.Report((int)prog);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                // TODO log
                return false;
            }
        }

        // This is 2-5 times faster
        #region MyDownloader

        public static Task DownloadAndExtractOpenSourceMinersWithMyDownloaderAsync(IProgress<Tuple<string, int>> progress, CancellationToken stop)
        {
            return DownloadAndExtractWithMyDownloaderAsync(StandardDlSetup, progress, stop);
        }

        public static Task DownloadAndExtractThirdPartyMinersWithMyDownloaderAsync(IProgress<Tuple<string, int>> progress, CancellationToken stop)
        {
            return DownloadAndExtractWithMyDownloaderAsync(ThirdPartyDlSetup, progress, stop);
        }

        private static async Task DownloadAndExtractWithMyDownloaderAsync(DownloadSetup downloadSetup, IProgress<Tuple<string, int>> progress, CancellationToken stop)
        {
            // these extensions must be here otherwise it will not downlaod
            var extensions = new List<IExtension>();
            try
            {
                extensions.Add(new CoreExtention());
                extensions.Add(new HttpFtpProtocolExtension());
            }
            catch { }

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
                Helpers.ConsolePrint("MinersDownloadManager", e.Message);
            }

            try
            {
                var downloadedSuccess = await DownlaodAsync(downloadSetup.BinsDownloadUrl, downloadSetup.BinsZipLocation, progress, stop);
                if (!downloadedSuccess || stop.IsCancellationRequested) {
                    Helpers.ConsolePrint("MinersDownloadManager", $"Download success={downloadedSuccess} || cancel={stop.IsCancellationRequested}");
                    return;
                }

                var zipFileExists = false;
                const int maxWait = 3000;
                const int stepWait = 300;
                int waitLeft = maxWait;
                while (waitLeft > 0)
                {
                    waitLeft -= stepWait;
                    if (File.Exists(downloadSetup.BinsZipLocation))
                    {
                        zipFileExists = true;
                        break;
                    }
                    await Task.Delay(stepWait);
                }
                if (!zipFileExists)
                {
                    Helpers.ConsolePrint("MinersDownloadManager", $"Downloaded file {downloadSetup.BinsZipLocation} doesn't exist exiting");
                    return;
                }

                var unzipProgress = new Progress<int>(perc => progress?.Report(new Tuple<string, int>(Translations.Tr("Unzipping {0} %", perc), perc)));
                if (downloadSetup.BinsZipLocation.EndsWith(".7z"))
                {
                    await Un7zipFileAsync(downloadSetup.BinsZipLocation, "miner_plugins", unzipProgress, stop);
                }
                else
                {
                    await UnzipFileAsync(downloadSetup.BinsZipLocation, "miner_plugins", unzipProgress, stop);
                }
                
                await Task.Delay(300);
                if (File.Exists(downloadSetup.BinsZipLocation))
                {
                    File.Delete(downloadSetup.BinsZipLocation);
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("MinersDownloadManager", $"Exception while downloading and extracting {e}");
            }
        }

        private static Downloader CreateDownloader(string url, string downloadLocation)
        {
            var location = ResourceLocation.FromURL(url);
            var mirrors = new ResourceLocation[0];
            var downloader = DownloadManager.Instance.Add(
                location,
                mirrors,
                downloadLocation,
                10,
                true);

            return downloader;
        }

        private delegate void OnDownloadEnded(object sender, DownloaderEventArgs e);

        // #2 download the file
        private static async Task<bool> DownlaodAsync(string url, string downloadLocation, IProgress<Tuple<string, int>> progress, CancellationToken stop)
        {
            long lastProgress = 0;
            var ticksSinceUpdate = 0;
            bool _isDownloadSizeInit = false;
            var downloader = CreateDownloader(url, downloadLocation);

            var timer = new Timer((object stateInfo) =>
            {
                if (downloader.State != DownloaderState.Working) return;
                if (!_isDownloadSizeInit)
                {
                    _isDownloadSizeInit = true;
                }

                if (downloader.LastError != null)
                {
                    Helpers.ConsolePrint("MinersDownloadManager", downloader.LastError.Message);
                }

                var speedString = $"{downloader.Rate / 1024d:0.00} kb/s";
                var percString = downloader.Progress.ToString("0.00") + "%";
                var labelDownloaded =
                    $"{downloader.Transfered / 1024d / 1024d:0.00} MB / {downloader.FileSize / 1024d / 1024d:0.00} MB";
                
                var progPerc = (int)(((double)downloader.Transfered / downloader.FileSize) * 100);
                var progMessage = $"{speedString}   {percString}   {labelDownloaded}";
                progress.Report(new Tuple<string, int>(progMessage, progPerc));

                // Diagnostic stuff
                if (downloader.Transfered > lastProgress)
                {
                    ticksSinceUpdate = 0;
                    lastProgress = downloader.Transfered;
                }
                else if (ticksSinceUpdate > 20)
                {
                    Helpers.ConsolePrint("MinersDownloadManager", "Maximum ticks reached, retrying");
                    ticksSinceUpdate = 0;
                }
                else
                {
                    Helpers.ConsolePrint("MinersDownloadManager", "No progress in ticks " + ticksSinceUpdate);
                    ticksSinceUpdate++;
                }
            });
            timer.Change(0, 500);

            stop.Register(() => {
                DownloadManager.Instance.RemoveDownload(downloader);
                timer.Dispose();
            });

            var tcs = new TaskCompletionSource<bool>();
            var onDownloadEnded = new EventHandler<DownloaderEventArgs>((object sender, DownloaderEventArgs e) =>
            {
                timer.Dispose();
                if (downloader != null)
                {
                    if (downloader.State == DownloaderState.EndedWithError)
                    {
                        Helpers.ConsolePrint("MinersDownloadManager", downloader.LastError.Message);
                        tcs.SetResult(false);
                    }
                    else if (downloader.State == DownloaderState.Ended)
                    {
                        Helpers.ConsolePrint("MinersDownloadManager", "DownloadCompleted Success");
                        tcs.SetResult(true);
                    }
                }
            });
            DownloadManager.Instance.DownloadEnded += onDownloadEnded;
            var result = await tcs.Task;
            DownloadManager.Instance.DownloadEnded -= onDownloadEnded;

            return result;
        }
        #endregion MyDownloader
    }
}
