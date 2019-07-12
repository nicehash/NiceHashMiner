using NiceHashMiner.Configs;
using System;
using System.Windows.Forms;
using NHM.Common.Enums;

namespace NiceHashMiner.Forms
{
    public partial class Form_3rdParty_TOS : Form
    {
        public Form_3rdParty_TOS()
        {
            InitializeComponent();
            // TODO update 3rd party TOS
            FormHelpers.TranslateFormControls(this);
        }


        private void Button_Agree_Click(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.Use3rdPartyMiners = Use3rdPartyMiners.YES;
            Close();
        }

        private void Button_Decline_Click(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.Use3rdPartyMiners = Use3rdPartyMiners.NO;
            Close();
        }
    }
}
