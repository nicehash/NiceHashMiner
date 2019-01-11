using System;
using System.Windows.Forms;

namespace NiceHashMiner
{
    // TODO probably remove
    public partial class MinerFileNotFoundDialog : Form
    {
        public bool DisableDetection;

        public MinerFileNotFoundDialog(string minerDeviceName, string path)
        {
            InitializeComponent();

            DisableDetection = false;
            Text = Translations.Tr("File not found!");
            linkLabelError.Text = string.Format(Translations.Tr("MinerFileNotFoundDialog_linkLabelError"),
                minerDeviceName, path, Translations.Tr("MinerFileNotFoundDialog_link"));
            linkLabelError.LinkArea =
                new LinkArea(linkLabelError.Text.IndexOf(Translations.Tr("MinerFileNotFoundDialog_link")),
                    Translations.Tr("MinerFileNotFoundDialog_link").Length);
            chkBoxDisableDetection.Text = Translations.Tr("MinerFileNotFoundDialog_chkBoxDisableDetection");
            buttonOK.Text = Translations.Tr("&OK");
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (chkBoxDisableDetection.Checked)
                DisableDetection = true;

            Close();
        }

        private void LinkLabelError_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/nicehash/NiceHashMiner#troubleshooting");
        }
    }
}
