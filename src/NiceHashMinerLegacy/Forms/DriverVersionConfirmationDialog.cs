using System;
using System.Windows.Forms;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Forms
{
    public partial class DriverVersionConfirmationDialog : Form
    {
        public DriverVersionConfirmationDialog()
        {
            InitializeComponent();

            Text = Translations.Tr("Update AMD Driver Recommended");
            labelWarning.Text = Translations.Tr("You're not using the optimal AMD Driver version. The most stable driver for mining is the 15.7.1 version.\nWe strongly suggest you to use this driver version.");
            linkToDriverDownloadPage.Text =
                Translations.Tr("&Link to Driver Download Page");
            chkBoxDontShowAgain.Text = Translations.Tr("&Do not show this warning again");
            buttonOK.Text = Translations.Tr("&OK");
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (chkBoxDontShowAgain.Checked)
            {
                Helpers.ConsolePrint("NICEHASH", "Setting ShowDriverVersionWarning to false");
                ConfigManager.GeneralConfig.ShowDriverVersionWarning = false;
            }

            Close();
        }

        private void LinkToDriverDownloadPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(
                "http://support.amd.com/en-us/download/desktop/legacy?product=legacy3&os=Windows+7+-+64");
        }
    }
}
