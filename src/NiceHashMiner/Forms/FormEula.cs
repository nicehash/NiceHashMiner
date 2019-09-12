using System;
using System.Diagnostics;
using System.Windows.Forms;
using NHMCore;
using NHMCore.Configs;

namespace NiceHashMiner.Forms
{
    public partial class FormEula : Form
    {
        public bool AcceptedToS { get; private set; } = false;
        public FormEula()
        {
            InitializeComponent();
            CenterToScreen();
            Icon = NHMCore.Properties.Resources.logo;
            InitializeTosComponent();
            FormHelpers.TranslateFormControls(this);
        }

        private void InitializeTosComponent()
        {
            richTextBoxToS.Rtf = NHMCore.Properties.Resources.Eula;
            richTextBoxToS.ReadOnly = true;
            richTextBoxToS.DetectUrls = true;
            richTextBoxToS.HideSelection = true;
            richTextBoxToS.LinkClicked += (s, e) => Process.Start(e.LinkText);
        }

        private void buttonAcceptToS_Click(object sender, EventArgs e)
        {
            AcceptedToS = true;
            ConfigManager.GeneralConfig.agreedWithTOS = ApplicationStateManager.CurrentTosVer;
            ConfigManager.GeneralConfigFileCommit();
            Close();
        }
    }
}
