using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public partial class PluginInfoItem : UserControl
    {
        public PluginInfoItem()
        {
            InitializeComponent();

            buttonInstallRemove.Click += new EventHandler(ButtonInstallRemove_Click);
            buttonUpdate.Click += new EventHandler(ButtonUpdate_Click);
            buttonCancel.Click += new EventHandler(ButtonCancel_Click);


            labelStatus.Text = "";
            buttonCancel.Visible = false;
            ProgressBarVisible = false;
        }

        public string Description
        {
            get
            {
                return labelShortendDescription.Text;
            }
            set
            {
                if (value == null) return;
                var setValue = value;
                if (setValue.Length > 150) setValue = setValue.Substring(0, 15) + "...";
                labelShortendDescription.Text = setValue;
            }
        }

        public string PluginName {
            get
            {
                return labelName.Text;
            }
            set
            {
                labelName.Text = value;
            }
        }

        public string PluginVersion
        {
            get
            {
                return labelVersion.Text;
            }
            set
            {
                labelVersion.Text = value;
            }
        }

        public string PluginAuthor
        {
            get
            {
                return labelAuthor.Text;
            }
            set
            {
                labelAuthor.Text = value;
            }
        }

        public string ButtonInstallRemoveText
        {
            get
            {
                return buttonInstallRemove.Text;
            }
            set
            {
                buttonInstallRemove.Text = value;
            }
        }

        public bool ButtonInstallRemoveEnabled
        {
            get
            {
                return buttonInstallRemove.Enabled;
            }
            set
            {
                buttonInstallRemove.Enabled = value;
            }
        }

        public string ButtonUpdateText
        {
            get
            {
                return buttonUpdate.Text;
            }
            set
            {
                buttonUpdate.Text = value;
            }
        }

        public bool ButtonUpdateVisible
        {
            get
            {
                return buttonUpdate.Visible;
            }
            set
            {
                buttonUpdate.Visible = value;
            }
        }

        public bool ProgressBarVisible
        {
            get
            {
                return progressBar1.Visible;
            }
            set
            {
                progressBar1.Value = 0;
                progressBar1.Visible = value;
            }
        }

        public int ProgressBarValue
        {
            get
            {
                return progressBar1.Value;
            }
            set
            {
                progressBar1.Value = value;
            }
        }

        public string StatusText
        {
            get
            {
                return labelStatus.Text;
            }
            set
            {
                labelStatus.Text = value;
            }
        }

        public bool StatusVisible
        {
            get
            {
                return labelStatus.Visible;
            }
            set
            {
                labelStatus.Visible = value;
            }
        }

        public void SwapInstallRemoveButtonWithCancelButton(bool isCancel)
        {
            if (isCancel)
            {
                buttonCancel.Visible = true;
                buttonInstallRemove.Enabled = false;
                buttonInstallRemove.Visible = false;
            }
            else
            {
                buttonCancel.Visible = false;
                buttonInstallRemove.Enabled = true;
                buttonInstallRemove.Visible = true;
            }
        }

        public string PluginUUID { get; set; }


        // Events TODO 
        public EventHandler<string> OnPluginInfoItemMouseClick;

        private void PluginInfoItem_MouseClick(object sender, MouseEventArgs e)
        {
            OnPluginInfoItemMouseClick?.Invoke(this, PluginUUID);
        }
        
        public EventHandler<string> OnButtonInstallRemoveClick;

        private void ButtonInstallRemove_Click(object sender, EventArgs e)
        {
            OnButtonInstallRemoveClick?.Invoke(this, PluginUUID);
        }

        public EventHandler<string> OnButtonCancel;

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            OnButtonCancel?.Invoke(this, PluginUUID);
        }

        public EventHandler<string> OnButtonUpdateClick;

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            OnButtonUpdateClick?.Invoke(this, PluginUUID);
        }

        public EventHandler<string> OnDetailsClick;

        private void linkLabelDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OnDetailsClick?.Invoke(this, PluginUUID);
        }
    }
}
