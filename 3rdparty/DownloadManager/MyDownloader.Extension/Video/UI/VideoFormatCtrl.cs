using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.Video.UI
{
    public partial class VideoFormatCtrl : UserControl
    {
        public VideoFormatCtrl()
        {
            InitializeComponent();

            UpdateUI();
            cboTypes.SelectedIndex = 0;
        }


        public event EventHandler Change;

        public VideoFormat VideoFormat
        {
            get 
            {
                if (chkConvert.Checked)
                {
                    if (cboTypes.SelectedIndex == 0) return VideoFormat.AVI;
                    if (cboTypes.SelectedIndex == 1) return VideoFormat.MPEG;
                    return VideoFormat.MP3;
                }
                return VideoFormat.None;
            }
            set 
            {
                chkConvert.Checked = value != VideoFormat.None;
                switch (value)
                {
                   case VideoFormat.AVI:
                       cboTypes.SelectedIndex = 0;
                        break;
                    case VideoFormat.MPEG:
                        cboTypes.SelectedIndex = 1;
                        break;
                    case VideoFormat.MP3:
                        cboTypes.SelectedIndex = 2;
                        break;
                }
            }
        }

        protected virtual void OnChange()
        {
            if (Change != null)
            {
                Change(this, EventArgs.Empty);
            }
        }

        private void chkConvert_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();

            OnChange();
        }

        private void UpdateUI()
        {
            cboTypes.Enabled = chkConvert.Checked;
        }

        private void cboTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnChange();
        }
    }
}
