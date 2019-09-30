using System;
using System.Windows.Forms;
using NHMCore;
using NHMCore.Configs;

namespace NiceHashMiner.Forms
{
    public partial class Form_3rdParty_TOS : Form
    {
        public bool Accepted { get; private set; } = false;
        public Form_3rdParty_TOS()
        {
            InitializeComponent();
            // TODO update 3rd party TOS
            FormHelpers.TranslateFormControls(this);
        }


        private void Button_Agree_Click(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.Use3rdPartyMinersTOS = ApplicationStateManager.CurrentTosVer;
            Accepted = true;
            Close();
        }

        private void Button_Decline_Click(object sender, EventArgs e)
        {
            Accepted = false;
            Close();
        }
    }
}
