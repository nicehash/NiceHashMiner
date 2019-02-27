using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using NiceHashMiner.Plugin;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;

namespace NiceHashMiner.Forms
{
    public partial class DownloadFormTest : Form
    {
        PluginPackageInfo pluginInfo = new PluginPackageInfo()
        {
            PluginUUID = "066745f3-6738-4b65-adbb-0d1e153ed873",
            PluginVersion = new Version(1, 0),
            PluginAuthor = "S74nk0@NiceHash",
            PluginDescription = "Plugin that runs GMiner",
            MinerPackageURL = "https://github.com/develsoftware/GMinerRelease/releases/download/1.34/gminer_1_34_minimal_windows64.zip",
            SupportedDevices = new List<string> { "NVIDIA SM5.0+" },
            PluginPackageURL = "N/A",
        };

        public DownloadFormTest()
        {
            InitializeComponent();
        }

        CancellationTokenSource cancelDownload;

        public static async Task<bool> DownloadFile(string url, string downloadFileLocation, DownloadProgressChangedEventHandler downloadProgressChangedEventHandler, CancellationToken stop)
        {
            var downloadStatus = false;
            using (var client = new WebClient()) {
                client.DownloadProgressChanged += downloadProgressChangedEventHandler;
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

        public delegate void OnZipProgres(int prog);

        public static async Task<bool> UnzipFile(string zipLocation, string unzipLocation, OnZipProgres zipProgressChangedEventHandler, CancellationToken stop)
        {
            return await Task.Run(() => {
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
                        zipProgressChangedEventHandler((int)prog);

                        var extractPath = Path.Combine(unzipLocation, entry.FullName);
                        var dirPath = Path.GetDirectoryName(extractPath);
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                        }
                        entry.ExtractToFile(extractPath, true);
                    }
                }
                return true;
            }, stop);
        }

        private void SetProgressBar(int prog) {
            SafeInvoke(() => { progressBar1.Value = prog; });
        }

        private void SetText(string text)
        {
            SafeInvoke(() => { button1.Text = text; });
        }

        private void SafeInvoke(Action f, bool beginInvoke = false)
        {
            if (InvokeRequired)
            {
                if (beginInvoke)
                {
                    BeginInvoke(f);
                }
                else
                {
                    Invoke(f);
                }
            }
            else
            {
                f();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            cancelDownload = new CancellationTokenSource();
            button1.Enabled = false;
            // progress delegates
            var onDownloadProgresChanged = new DownloadProgressChangedEventHandler((s, e1) => { SetProgressBar(e1.ProgressPercentage); });
            OnZipProgres onUnzipChanged = (int progress) => {
                SetProgressBar(progress);
                SetText($"Unzipping {progress.ToString("F2")} %");
            };

            // create the directory if it doesn't exist
            var pluginRootPath = Path.Combine(Paths.MinerPluginsPath(), pluginInfo.PluginUUID);
            Directory.CreateDirectory(Path.GetDirectoryName(pluginRootPath));

            // save the plugin info file
            var jsonString = JsonConvert.SerializeObject(pluginInfo, Formatting.Indented);
            File.WriteAllText(Path.Combine(pluginRootPath, "PluginInfo.json"), jsonString, Encoding.UTF8);

            // download plugin dll
            var pluginPackageDownloaded = Path.Combine(pluginRootPath, "plugin.zip");
            var pluginURL = pluginInfo.PluginPackageURL;
            SetText("Downloading miner PLUGIN");
            SetProgressBar(0);
            var pluginDownloaded = await DownloadFile(pluginURL, pluginPackageDownloaded, onDownloadProgresChanged, cancelDownload.Token);
            if (pluginDownloaded)
            {
                button1.Text = "FAILED PLUGIN DOWNLOAD";
                button1.Enabled = true;
                return;
            }
            if (!File.Exists(pluginPackageDownloaded))
            {
                button1.Text = "Zip file deleted";
                button1.Enabled = true;
                return;
            }
            var pluginExctracted = await UnzipFile(pluginPackageDownloaded, pluginRootPath, onUnzipChanged, cancelDownload.Token);
            if (!pluginExctracted) {
                button1.Text = "plugin EXTRACTION FAILED";
                button1.Enabled = true;
                return;
            }

            // download miners scope
            var downloadLocation = Path.Combine(pluginRootPath, "bins.zip"); ;
            var url1 = pluginInfo.MinerPackageURL;
            SetText("Downloading miner bins");
            SetProgressBar(0);
            var downloaded = await DownloadFile(url1, downloadLocation, onDownloadProgresChanged, cancelDownload.Token);
            Console.WriteLine(downloaded ? $"Download GOOD" : $"Download BAD");
            SetProgressBar(0);
            if (downloaded && File.Exists(downloadLocation)) {
                
                var extractPath = Path.Combine(Environment.CurrentDirectory, "unzipped_folder");
                await UnzipFile(downloadLocation, extractPath, onUnzipChanged, cancelDownload.Token);
            }

            button1.Text = "Download FINISHED";
            button1.Enabled = true;
        }

    }
}
