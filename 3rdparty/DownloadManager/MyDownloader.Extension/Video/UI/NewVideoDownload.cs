using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MyDownloader.Core;
using MyDownloader.Extension.Video;
using System.Diagnostics;
using System.IO;
using MyDownloader.Core.Common;
using MyDownloader.Core.UI;
using MyDownloader.Extension.Video.Impl;
using System.Threading;

namespace MyDownloader.Extension.Video.UI
{
    public partial class NewVideoDownload : Form
    {
        bool hasSet = false;
        VideoDownloadHandler handler;
        VideoDownloadExtension extension;
        Thread videoTitleReaderThread;

        public NewVideoDownload()
        {
            InitializeComponent();

            extension = (VideoDownloadExtension)AppManager.Instance.Application.GetExtensionByType(typeof(VideoDownloadExtension));

            videoFormatCtrl1.Change += new EventHandler(videoFormatCtrl1_Change);            
        }

        void videoFormatCtrl1_Change(object sender, EventArgs e)
        {
            UpdateFileExt();
        }

        private void UpdateFileExt()
        {
            string file = txtFilename.Text;

            if (!string.IsNullOrEmpty(file))
            {
                if (videoFormatCtrl1.VideoFormat != VideoFormat.None)
                {
                    file = Path.ChangeExtension(file, "." + videoFormatCtrl1.VideoFormat.ToString().ToLower());
                }
                else
                {
                    file = Path.ChangeExtension(file, ".flv");
                }

                txtFilename.Text = file;
            }
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            ReleaseVideoThread();

            handler = extension.GetHandlerByURL(txtURL.Text);
            if (handler == null)
            {
                btnOK.Enabled = false;
                pictureBox1.Image = null;
                return;
            }

            btnOK.Enabled = true;

            Type typeHandler = handler.Type;
            DisplayLogo(typeHandler);

            videoTitleReaderThread = new Thread(
                delegate(object state)
                {
                    object[] parms = (object[])state;

                    Type type = (Type)parms[0];
                    string url = (string)parms[1];

                    BaseVideoDownloader videoDownloader = (BaseVideoDownloader)Activator.CreateInstance(type);

                    string titile = videoDownloader.GetTitle(ResourceLocation.FromURL(url));

                    this.BeginInvoke((MethodInvoker)delegate() { txtFilename.Text = titile; UpdateFileExt(); waitControl1.Visible = false; });
                }
            );

            waitControl1.Visible = true;
            videoTitleReaderThread.Start(new object[] { typeHandler, txtURL.Text });
        }

        private void ReleaseVideoThread()
        {
            if (videoTitleReaderThread != null)
            {
                if (videoTitleReaderThread.IsAlive)
                {
                    videoTitleReaderThread.Abort();
                    videoTitleReaderThread = null;
                }                
            }

            waitControl1.Visible = false;
        }

        private void DisplayLogo(Type typeHandler)
        {
            string logoName = typeHandler.Namespace + ".Logos." + typeHandler.Name + ".png";

            using (Stream s = handler.Type.Assembly.GetManifestResourceStream(logoName))
            {
                pictureBox1.Image = Image.FromStream(s);
            }

            //Debug.WriteLine(logoName);
        }

        private void lblSites_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (SupportedVideoSitesForm sites = new SupportedVideoSitesForm())
            {
                sites.ShowDialog();
            }
        }

        public ResourceLocation DownloadLocation
        {
            get
            {
                ResourceLocation rl = ResourceLocation.FromURL(txtURL.Text);
                rl.ProtocolProviderType = handler.Type.AssemblyQualifiedName;

                return rl;
            }
            set
            {
                hasSet = true;
                if (value == null)
                {
                    txtURL.Clear();
                }
                else
                {
                    txtURL.Text = value.URL;
                }
            }
        }

        public string LocalFile
        {
            get
            {
                return PathHelper.GetWithBackslash(downloadFolder1.Folder) + txtFilename.Text;
            }
        }

        public int Segments
        {
            get
            {
                return (int)numSegments.Value;
            }
        }

        public bool StartNow
        {
            get
            {
                return chkStartNow.Checked;
            }
        }

        public VideoFormat VideoFormat
        {
            get { return videoFormatCtrl1.VideoFormat; }
        }

        private void txtFilename_Leave(object sender, EventArgs e)
        {
            UpdateFileExt();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            UpdateFileExt();

            AddDownloadToList();

            Close();
        }

        private void AddDownloadToList()
        {
            Downloader download = DownloadManager.Instance.Add(
                this.DownloadLocation,
                null,
                this.LocalFile,
                this.Segments,
                false);

            VideoConverter.SetConvertOption(download, this.VideoFormat);

            if (this.StartNow)
            {
                download.Start();
            }
        }

        private void NewVideoDownload_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReleaseVideoThread();
        }

        private void NewVideoDownload_Load(object sender, EventArgs e)
        {
            if (!hasSet)
            {
                txtURL.Text = ClipboardHelper.GetURLOnClipboard();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}