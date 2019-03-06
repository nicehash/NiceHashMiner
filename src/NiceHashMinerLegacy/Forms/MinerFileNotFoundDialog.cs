using NiceHashMiner.Forms;
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

            FormHelpers.TranslateFormControls(this);


            linkLabelError.Text = string.Format(Translations.Tr("{0}: File {1} is not found!\n\nPlease make sure that the file is accessible and that your anti-virus is not blocking the application.\nPlease refer the section \"My anti-virus is blocking the application\" at the Troubleshooting section ({2}).\n\nA re-download of NiceHash Miner Legacy might be needed."),
                minerDeviceName, path, Translations.Tr("Link"));
            linkLabelError.LinkArea =
                new LinkArea(linkLabelError.Text.IndexOf(Translations.Tr("Link")),
                    Translations.Tr("Link").Length);
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
