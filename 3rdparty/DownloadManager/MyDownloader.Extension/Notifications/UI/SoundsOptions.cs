using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.Notifications.UI
{
    public partial class SoundsOptions : UserControl
    {
        public SoundsOptions()
        {
            this.Text = "Sounds";

            InitializeComponent();

            soundChooserAdded.FileName = Settings.Default.DownloadAddedSound;
            soundChooserRemoved.FileName = Settings.Default.DownloadRemovedSound;
            soundChooserEnded.FileName = Settings.Default.DownloadEndedSound;
        }

        public string DownloadAdded
        {
            get
            {
                return soundChooserAdded.FileName;
            }
        }

        public string DownloadRemoved
        {
            get
            {
                return soundChooserRemoved.FileName;
            }
        }

        public string DownloadEnded
        {
            get
            {
                return soundChooserEnded.FileName;
            }
        }
    }
}