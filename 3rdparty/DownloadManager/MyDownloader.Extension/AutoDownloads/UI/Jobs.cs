using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.AutoDownloads.UI
{
    public partial class Jobs : UserControl
    {
        public Jobs()
        {
            InitializeComponent();

            Text = "Auto-Downloads";

            numMaxJobs.Value = Settings.Default.MaxJobs;
            chkUseTime.Checked = Settings.Default.WorkOnlyOnSpecifiedTimes;
            timeGrid1.SelectedTimes = new DayHourMatrix(Settings.Default.TimesToWork);
            numMaxRate.Value = (decimal)(Math.Max(Settings.Default.MaxRateOnTime, 1024) / 1024.0);
            chkAutoStart.Checked = Settings.Default.AutoStart;

            UpdateUI();
        }

        private void chkUseTime_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            pnlTime.Enabled = chkUseTime.Checked;
        }

        public int MaxJobs
        {
            get { return (int)numMaxJobs.Value; }
        }

        public double MaxRate
        {
            get { return ((double)numMaxRate.Value) * 1024; }
        }

        public bool WorkOnlyOnSpecifiedTimes
        {
            get { return chkUseTime.Checked; }
        }

        public bool AutoStart
        {
            get { return chkAutoStart.Checked; }
        }

        public string TimesToWork
        {
            get { return timeGrid1.SelectedTimes.ToString(); }
        }	
    }
}
