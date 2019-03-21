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
    public partial class PluginInfoItemRow : UserControl
    {
        public PluginInfoItemRow()
        {
            InitializeComponent();
        }

        public PluginInfoItem PluginInfoItem1
        {
            get
            {
                return pluginInfoItem1;
            }
        }

        public PluginInfoItem PluginInfoItem2
        {
            get
            {
                return pluginInfoItem2;
            }
        }
    }
}
