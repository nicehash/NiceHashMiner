using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NiceHashMiner.Configs;
using NiceHashMiner.Utils;

namespace NiceHashMiner.Forms
{
    public partial class EnterBTCDialogSwitch : Form
    {
        public EnterBTCDialogSwitch()
        {
            InitializeComponent();
            CenterToScreen();
            Icon = Properties.Resources.logo;
            Text += " v" + Application.ProductVersion;
            FormHelpers.TranslateFormControls(this);
        }

        private void SetChildFormCenter(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(Location.X + (Width - form.Width) / 2, Location.Y + (Height - form.Height) / 2);
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            var loginForm = new LoginForm();
            SetChildFormCenter(loginForm);
            loginForm.ShowDialog();
            if (CredentialValidators.ValidateBitcoinAddress(loginForm.Btc))
            {
                ConfigManager.GeneralConfig.BitcoinAddress = loginForm.Btc;
                ConfigManager.GeneralConfigFileCommit();
                Close();
            }
        }

        private void LinkButtonEnterBTCManually_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Close();
        }

        private void LinkButtonRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Links.Register);
        }
    }
}
