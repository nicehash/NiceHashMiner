using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.SpeedLimit.UI
{
    public partial class LimitCfg : UserControl
    {
        public LimitCfg()
        {
            InitializeComponent();

            Text = "Speed Limit";

            chkEnableLimit.Checked = Settings.Default.EnabledLimit;
            numMaxRate.Value = (decimal)(Math.Max(Settings.Default.MaxRate, 1024) / 1024.0);
            
            UpdateUI();
        }

        private void chkEnableLimit_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        public double MaxRate
        {
            get { return ((double)numMaxRate.Value) * 1024; }
        }

        public bool EnableLimit
        {
            get { return chkEnableLimit.Checked; }
        }

        private void UpdateUI()
        {
            lblMaxRate.Enabled = chkEnableLimit.Checked;
            numMaxRate.Enabled = chkEnableLimit.Checked;
        }
    }
}
