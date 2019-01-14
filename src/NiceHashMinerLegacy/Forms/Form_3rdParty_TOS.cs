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
            Text = Translations.Tr("Disclaimer on usage of 3rd party software");
            label_Tos.Text = Translations.Tr("We have integrated 3rd party mining software that should speed up your mining and give you more stable mining experience - this could potentially result in more earnings over a shorter period of time even after developer's fee is deducted. However, since this is 3rd party software that is fully closed-source, we have no chance to inspect it in any way. NiceHash can not vouch for using that software and is refusing to take any responsibility for any damage caused, security breaches, loss of data or funds, system or hardware error, and other issues. By agreeing to this disclaimer you take full responsibility for using these closed-source miners as they are.");
            button_Agree.Text = Translations.Tr("I agree");
            button_Decline.Text = Translations.Tr("I refuse");
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
