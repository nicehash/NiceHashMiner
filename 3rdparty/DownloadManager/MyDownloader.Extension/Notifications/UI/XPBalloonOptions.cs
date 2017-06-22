using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.Notifications.UI
{
    public partial class XPBalloonOptions : UserControl
    {
        public XPBalloonOptions()
        {
            this.Text = "Balloon";

            InitializeComponent();

            chkBallon.Checked = Settings.Default.ShowBallon;
            numDuration.Value = Settings.Default.BallonTimeout / 1000;
        }

        public bool ShowBallon
        {
            get { return chkBallon.Checked; }
        }

        public int BallonTimeout
        {
            get { return (int)numDuration.Value * 1000; }
        }
    }
}
