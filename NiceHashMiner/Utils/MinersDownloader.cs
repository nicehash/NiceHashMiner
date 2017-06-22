using NiceHashMiner.Interfaces;
using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Common;
using SharpCompress.Reader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using MyDownloader.Core;
using MyDownloader.Extension.Protocols;
using MyDownloader.Core.Extensions;
using MyDownloader.Core.UI;

namespace NiceHashMiner.Utils {
    public class MinersDownloader {
        private const string TAG = "MinersDownloader";

        DownloadSetup _downloadSetup;

        private Downloader downloader;
        private Timer timer;
        private int ticksSinceUpdate;
        private long lastProgress;
        Thread _UnzipThread = null;

        bool isDownloadSizeInit = false;

        IMinerUpdateIndicator _minerUpdateIndicator;

        public MinersDownloader(DownloadSetup downloadSetup) {
            _downloadSetup = downloadSetup;

            var extensions = new List<IExtension>();
            try {
                extensions.Add(new CoreExtention());
                extensions.Add(new HttpFtpProtocolExtension());
            } catch { }
        }

        public void Start(IMinerUpdateIndicator minerUpdateIndicator) {
            _minerUpdateIndicator = minerUpdateIndicator;

            // if something not right delete previous and download new
            try {
                if (File.Exists(_downloadSetup.BinsZipLocation)) {
                    File.Delete(_downloadSetup.BinsZipLocation);
                }
                if (Directory.Exists(_downloadSetup.ZipedFolderName)) {
                    Directory.Delete(_downloadSetup.ZipedFolderName, true);
                }
            } catch (Exception e) {
                Helpers.ConsolePrint("MinersDownloader", e.Message);
            }
            Downlaod();
        }

        // #2 download the file
        private void Downlaod() {
            lastProgress = 0;
            ticksSinceUpdate = 0;

            _minerUpdateIndicator.SetTitle(International.GetText("MinersDownloadManager_Title_Downloading"));

            DownloadManager.Instance.DownloadEnded += new EventHandler<DownloaderEventArgs>(DownloadCompleted);

            ResourceLocation location = ResourceLocation.FromURL(_downloadSetup.BinsDownloadURL);
            ResourceLocation[] mirrors = new ResourceLocation[0];

            downloader = DownloadManager.Instance.Add(
                location,
                mirrors,
                _downloadSetup.BinsZipLocation,
                10,
                true);

            timer = new Timer(tmrRefresh_Tick);
            timer.Change(0, 500);
        }

        #region Download delegates

        private void tmrRefresh_Tick(Object stateInfo) {
            if (downloader != null && downloader.State == DownloaderState.Working) {
                if (!isDownloadSizeInit) {
                    isDownloadSizeInit = true;
                    _minerUpdateIndicator.SetMaxProgressValue((int)(downloader.FileSize / 1024));
                }

                if (downloader.LastError != null) {
                    Helpers.ConsolePrint("MinersDownloader", downloader.LastError.Message);
                }

                var speedString = string.Format("{0} kb/s", (downloader.Rate / 1024d).ToString("0.00"));
                var percString = downloader.Progress.ToString("0.00") + "%";
                var labelDownloaded = string.Format("{0} MB / {1} MB",
                    (downloader.Transfered / 1024d / 1024d).ToString("0.00"),
                    (downloader.FileSize / 1024d / 1024d).ToString("0.00"));
                _minerUpdateIndicator.SetProgressValueAndMsg((int)(downloader.Transfered / 1024d),
                    String.Format("{0}   {1}   {2}", speedString, percString, labelDownloaded));

                // Diagnostic stuff
                if (downloader.Transfered > lastProgress) {
                    ticksSinceUpdate = 0;
                    lastProgress = downloader.Transfered;
                } else if (ticksSinceUpdate > 20) {
                    Helpers.ConsolePrint("MinersDownloader", "Maximum ticks reached, retrying");
                    ticksSinceUpdate = 0;
                } else {
                    Helpers.ConsolePrint("MinersDownloader", "No progress in ticks " + ticksSinceUpdate.ToString());
                    ticksSinceUpdate++;
                }
            }
        }

        // The event that will trigger when the WebClient is completed
        private void DownloadCompleted(object sender, DownloaderEventArgs e) {
            timer.Dispose();

            if (downloader != null) {
                if (downloader.State == DownloaderState.EndedWithError) {
                    Helpers.ConsolePrint("MinersDownloader", downloader.LastError.Message);
                } else if (downloader.State == DownloaderState.Ended) {
                    Helpers.ConsolePrint(TAG, "DownloadCompleted Success");
                    System.Threading.Thread.Sleep(100);
                    int try_count = 50;
                    while (!File.Exists(_downloadSetup.BinsZipLocation) && try_count > 0) { --try_count; }

                    UnzipStart();
                }
            }
        }

        #endregion Download delegates


        private void UnzipStart() {
            try {
                _minerUpdateIndicator.SetTitle(International.GetText("MinersDownloadManager_Title_Settup"));
            } catch {

            }
            _UnzipThread = new Thread(UnzipThreadRoutine);
            _UnzipThread.Start();
        }

        private void UnzipThreadRoutine() {
            try {
                if (File.Exists(_downloadSetup.BinsZipLocation)) {
                    
                    Helpers.ConsolePrint(TAG, _downloadSetup.BinsZipLocation + " already downloaded");
                    Helpers.ConsolePrint(TAG, "unzipping");

                    // if using other formats as zip are returning 0
                    FileInfo fileArchive = new FileInfo(_downloadSetup.BinsZipLocation);
                    var archive = ArchiveFactory.Open(_downloadSetup.BinsZipLocation);
                    _minerUpdateIndicator.SetMaxProgressValue(100);
                    long SizeCount = 0;
                    foreach (var entry in archive.Entries) {
                        if (!entry.IsDirectory) {
                            SizeCount += entry.CompressedSize;
                            Helpers.ConsolePrint(TAG, entry.Key);
                            entry.WriteToDirectory("", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);

                            double prog = ((double)(SizeCount) / (double)(fileArchive.Length) * 100);
                            _minerUpdateIndicator.SetProgressValueAndMsg((int)prog, String.Format(International.GetText("MinersDownloadManager_Title_Settup_Unzipping"), prog.ToString("F2")));
                        }
                    }
                    archive.Dispose();
                    archive = null;
                    // after unzip stuff
                    _minerUpdateIndicator.FinishMsg(true);
                    // remove bins zip
                    try {
                        if (File.Exists(_downloadSetup.BinsZipLocation)) {
                            File.Delete(_downloadSetup.BinsZipLocation);
                        }
                    } catch (Exception e) {
                        Helpers.ConsolePrint("MinersDownloader.UnzipThreadRoutine", "Cannot delete exception: " + e.Message);
                    }
                } else {
                    Helpers.ConsolePrint(TAG, String.Format("UnzipThreadRoutine {0} file not found", _downloadSetup.BinsZipLocation));
                }
            } catch (Exception e) {
                Helpers.ConsolePrint(TAG, "UnzipThreadRoutine has encountered an error: " + e.Message);
            }
        }
    }
}
