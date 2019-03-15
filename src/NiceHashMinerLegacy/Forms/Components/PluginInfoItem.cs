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

        public string ButtonText
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

        public EventHandler<string> OnPluginInfoItemButtonClick;

        private void button1_Click(object sender, EventArgs e)
        {
            OnPluginInfoItemButtonClick?.Invoke(this, PluginUUID);
        }
    }
}
