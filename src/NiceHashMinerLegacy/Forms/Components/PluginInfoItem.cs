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

            ProgressBarVisible = false;
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

        public bool ButtonUpdateEnabled
        {
            get
            {
                return buttonUpdate.Enabled;
            }
            set
            {
                buttonUpdate.Enabled = value;
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
                progressBar1.Invalidate();
            }
        }

        #region DEBUGGING
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
        #endregion DEBUGGING

        public string PluginUUID { get; set; }

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

        public EventHandler<string> OnButtonUpdateClick;

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            OnButtonUpdateClick?.Invoke(this, PluginUUID);
        }
    }
}
