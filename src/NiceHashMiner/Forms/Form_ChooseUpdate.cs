using System;
using System.Windows.Forms;
using NHM.Common;
using NHMCore;
using NHMCore.Utils;

namespace NiceHashMiner.Forms
{
    public partial class Form_ChooseUpdate : Form
    {
        public Form_ChooseUpdate()
        {
            InitializeComponent();
            Icon = NHMCore.Properties.Resources.logo;
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

                await UpdateHelpers.DownloadUpdaterAsync(downloadProgress);
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
