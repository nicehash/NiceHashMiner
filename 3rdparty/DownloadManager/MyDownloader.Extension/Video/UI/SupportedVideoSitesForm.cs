using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MyDownloader.Extension.Video;
using System.IO;
using MyDownloader.Core.UI;

namespace MyDownloader.Extension.Video.UI
{
    public partial class SupportedVideoSitesForm : Form
    {
        VideoDownloadExtension extension;

        public SupportedVideoSitesForm()
        {
            InitializeComponent();
        }

        private void SupportedVideoSitesForm_Load(object sender, EventArgs e)
        {
            extension = (VideoDownloadExtension)AppManager.Instance.Application.GetExtensionByType(typeof(VideoDownloadExtension));

            for (int i = 0; i < extension.Handlers.Count; i++)
            {
                this.lstSites.Items.Add(extension.Handlers[i]);
            }

            if (this.lstSites.Items.Count > 0)
            {
                this.lstSites.SelectedIndex = 0;
            }
        }

        private void lstSites_SelectedIndexChanged(object sender, EventArgs e)
        {
            VideoDownloadHandler handler = (VideoDownloadHandler)lstSites.Items[lstSites.SelectedIndex];
            Type t = handler.Type;
            string logoName = t.Namespace + ".Logos." + t.Name + ".png";

            using (Stream s = t.Assembly.GetManifestResourceStream(logoName))
            {
                pictureBox1.Image = Image.FromStream(s);
            }
        }
    }
}