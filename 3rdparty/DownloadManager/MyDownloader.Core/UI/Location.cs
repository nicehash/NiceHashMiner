using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MyDownloader.Core;
using MyDownloader.Core.UI;

namespace MyDownloader.App.UI
{
    public partial class Location : UserControl
    {
        private bool hasSet = false;

        public Location()
        {
            InitializeComponent();

            Clear();
        }

        public event EventHandler UrlChanged;

        public string UrlLabelTitle
        {
            get
            {
                return lblURL.Text;
            }
            set
            {
                lblURL.Text = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ResourceLocation ResourceLocation
        {
            get 
            {
                ResourceLocation rl = new ResourceLocation();
                rl.Authenticate = chkLogin.Checked;
                rl.Login = txtLogin.Text;
                rl.Password = txtPass.Text;
                rl.URL = txtURL.Text;

                return rl;
            }
            set
            {
                hasSet = true;

                if (value != null)
                {
                    chkLogin.Checked = value.Authenticate;
                    txtLogin.Text = value.Login;
                    txtPass.Text = value.Password;
                    txtURL.Text = value.URL;
                }
                else
                {
                    chkLogin.Checked = false;
                    txtLogin.Text = String.Empty;
                    txtPass.Text = String.Empty;
                    txtURL.Text = String.Empty;
                }
            }
        }

		public void Clear()
		{
            txtURL.Text = string.Empty;
            chkLogin.Checked = false;
            txtPass.Text = string.Empty;
            txtLogin.Text = string.Empty;
            UpdateUI();
		}

        private void chkLogin_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            lblLogin.Enabled = chkLogin.Checked;
            lblPass.Enabled = chkLogin.Checked;
            txtLogin.Enabled = chkLogin.Checked;
            txtPass.Enabled = chkLogin.Checked;
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            if (UrlChanged != null)
            {
                UrlChanged(this, EventArgs.Empty);
            }
        }

        private void Location_Load(object sender, EventArgs e)
        {
            if (! hasSet)
            {
                txtURL.Text = ClipboardHelper.GetURLOnClipboard();
            }
        }
    }
}
