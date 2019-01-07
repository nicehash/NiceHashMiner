using NiceHashMiner.Configs;
using System;
using System.Windows.Forms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Forms
{
    public partial class Form_ChooseLanguage : Form
    {
        public Form_ChooseLanguage()
        {
            InitializeComponent();

            // Add language selections list
            var lang = International.GetAvailableLanguages();

            comboBox_Languages.Items.Clear();
            for (var i = 0; i < lang.Count; i++)
            {
                comboBox_Languages.Items.Add(lang[(LanguageType) i]);
            }

            comboBox_Languages.SelectedIndex = 0;

            //label_Instruction.Location = new Point((this.Width - label_Instruction.Size.Width) / 2, label_Instruction.Location.Y);
            //button_OK.Location = new Point((this.Width - button_OK.Size.Width) / 2, button_OK.Location.Y);
            //comboBox_Languages.Location = new Point((this.Width - comboBox_Languages.Size.Width) / 2, comboBox_Languages.Location.Y);
            textBox_TOS.Text = Eula.Text;
            textBox_TOS.ScrollBars = ScrollBars.Vertical;
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.Language = (LanguageType) comboBox_Languages.SelectedIndex;
            ConfigManager.GeneralConfigFileCommit();
            Close();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_TOS.Checked)
            {
                ConfigManager.GeneralConfig.agreedWithTOS = Globals.CurrentTosVer;
                comboBox_Languages.Enabled = true;
                button_OK.Enabled = true;
            }
            else
            {
                ConfigManager.GeneralConfig.agreedWithTOS = 0;
                comboBox_Languages.Enabled = false;
                button_OK.Enabled = false;
            }
        }
    }
}
