using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.WindowsIntegration
{
    public partial class WindowsIntegrationUI : UserControl
    {
        public WindowsIntegrationUI()
        {
            InitializeComponent();

            this.Text = "Windows Integration";

            chkStartWithWindows.Checked = WindowsStartupUtility.IsRegistered();
            chkMonitorWindowsClipboard.Checked = Settings.Default.MonitorClipboard;
        }

        public bool MonitorClipboard
        {
            get
            {
                return chkMonitorWindowsClipboard.Checked;
            }
        }

        public bool StartWithWindows
        {
            get
            {
                return chkStartWithWindows.Checked;
            }
        }
    }
}
