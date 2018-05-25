using NiceHashMiner.Configs;
using System;
using System.Windows.Forms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Forms
{
    public partial class Form_3rdParty_TOS : Form
    {
        public Form_3rdParty_TOS()
        {
            InitializeComponent();

            // TODO update 3rd party TOS
            Text = International.GetText("Form_Main_3rdParty_Title");
            label_Tos.Text = International.GetText("Form_Main_3rdParty_Text");
            button_Agree.Text = International.GetText("Form_Main_3rdParty_Button_Agree_Text");
            button_Decline.Text = International.GetText("Form_Main_3rdParty_Button_Refuse_Text");
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
