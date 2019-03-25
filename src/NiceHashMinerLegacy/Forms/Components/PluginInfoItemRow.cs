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

        public void SetPluginInfoItem1(PluginInfoItem pluginInfoItem)
        {
            RemovePluginInfoItem1();
            pluginInfoItem.Location = new Point(3, 3);
            Controls.Add(pluginInfoItem);
            pluginInfoItem1 = pluginInfoItem;
        }

        public void SetPluginInfoItem2(PluginInfoItem pluginInfoItem)
        {
            RemovePluginInfoItem2();
            pluginInfoItem.Location = new Point(283, 3);
            Controls.Add(pluginInfoItem);
            pluginInfoItem2 = pluginInfoItem;
        }

        public void RemovePluginInfoItem1()
        {
            if (pluginInfoItem1 != null)
            {
                Controls.Remove(pluginInfoItem1);
            }
        }

        public void RemovePluginInfoItem2()
        {
            if (pluginInfoItem2 != null)
            {
                Controls.Remove(pluginInfoItem2);
            }
        }
    }
}
