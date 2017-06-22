using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.AntiVirus.UI
{
    public partial class AVOptions : UserControl
    {
        public AVOptions()
        {
            InitializeComponent();

            this.Text = "Options";

            CheckFileWithAV = Settings.Default.CheckFileWithAV;
            AVFileName = Settings.Default.AVFileName;
            FileTypes = Settings.Default.FileTypes;
            AVParameter = Settings.Default.AVParameter;

            UpdateControls();
        }

        public bool CheckFileWithAV
        {
            get
            {
                return chkAllowAV.Checked;
            }
            set
            {
                chkAllowAV.Checked = value;
            }
        }

        public string AVFileName
        {
            get
            {
                return txtAVFileName.Text;
            }
            set
            {
                txtAVFileName.Text = value;
            }
        }

        public string FileTypes
        {
            get
            {
                return txtFileTypes.Text;
            }
            set
            {
                txtFileTypes.Text = value;
            }
        }

        public string AVParameter
        {
            get
            {
                return txtParameter.Text;
            }
            set
            {
                txtParameter.Text = value;
            }
        }

        private void chkAllowAV_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            btnSelAV.Enabled = chkAllowAV.Checked;
            txtAVFileName.Enabled = chkAllowAV.Checked;
            txtFileTypes.Enabled = chkAllowAV.Checked;
            txtParameter.Enabled = chkAllowAV.Checked;
        }

        private void btnSelAV_Click(object sender, EventArgs e)
        {
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                txtAVFileName.Text = openFileDlg.FileName;
            }
        }
    }
}
