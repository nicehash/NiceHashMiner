using NHM.Common;
using NHM.Common.Enums;
using NHM.MinersDownloader;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Mining;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static NiceHashMiner.Mining.Plugins.MinerPluginsManager;

namespace NiceHashMiner.Forms
{
    public partial class Form_ChooseUpdate : Form
    {
        public Form_ChooseUpdate()
        {
            InitializeComponent();
            FormHelpers.TranslateFormControls(this);
            ProgressBarVisible = false;
            progressBar1.Width = this.Width;
        }

        public bool ProgressBarVisible
        {
            get
            {
                return progressBar1.Visible;
            }
            set
            {
                progressBar1.Value = 0;
                progressBar1.Visible = value;
            }
        }

        private async void UpdaterBtn_Click(object sender, EventArgs e)
        {
            UpdaterBtn.Enabled = false;
            GithubBtn.Enabled = false;
            ProgressBarVisible = true;
            try
            {
                var progressDownload = new Progress<(string loadMessageText, int perc)>(p =>
                {
                    progressBar1.Value = p.perc;
                    label1.Text = p.loadMessageText;
                });
                IProgress<(string loadMessageText, int prog)> progress = progressDownload;
                var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr($"Downloading updater: %{perc}"), perc)));

                var url = "https://github.com/luc1an24/pluginTesting/releases/download/1.0.1/nhm_windows_updater_1.9.2.12.exe";
                var downloadRootPath = Path.GetTempPath();
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
            }
            catch (Exception ex)
            {
                Logger.Error("Updater", $"Updating failed: {ex.Message}");
            }

            ProgressBarVisible = false;
            UpdaterBtn.Enabled = true;
            GithubBtn.Enabled = true;

            Close();
        }

        private void GithubBtn_Click(object sender, EventArgs e)
        {
            Close();
            ApplicationStateManager.VisitNewVersionUrl();
        }
    }
}
