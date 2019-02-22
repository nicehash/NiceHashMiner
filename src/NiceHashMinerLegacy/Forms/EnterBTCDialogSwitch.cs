using System;
using System.Windows.Forms;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Forms
{
    public partial class EnterBTCDialogSwitch : Form
    {
        public bool IsLogin { get; protected set; } = false;
        public EnterBTCDialogSwitch()
        {
            InitializeComponent();
            CenterToScreen();
            Text += " v" + Application.ProductVersion;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            IsLogin = true;
            Close();
        }

        private void LinkToDriverDownloadPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Close();
        }
    }
}
