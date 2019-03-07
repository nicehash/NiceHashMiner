using MinerPlugin;
using NiceHashMiner.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public partial class Form_MinerPlugins : Form
    {
        public Form_MinerPlugins()
        {
            InitializeComponent();
            CenterToScreen();
            Icon = Properties.Resources.logo;
            FormHelpers.TranslateFormControls(this);

            this.Shown += new EventHandler(this.FormShown);
        }

        private void FormShown(object sender, EventArgs e)
        {
            foreach (var plugin in MinerPluginsManager.OnlinePlugins)
            {
                dataGridView1.Rows.Add(GetPluginRowData(plugin));

                var newRow = dataGridView1.Rows[dataGridView1.Rows.Count - 1];
                newRow.Tag = plugin;
            }

            foreach (var kvp in MinerPluginsManager.MinerPlugin)
            {
                
                dataGridView1.Rows.Add(GetPluginRowData(kvp.Value));

                var newRow = dataGridView1.Rows[dataGridView1.Rows.Count - 1];
                newRow.Tag = kvp.Value;
            }
        }

        public static object[] GetPluginRowData(PluginPackageInfo p)
        {
            object[] rowData = { p.PluginUUID, p.PluginAuthor, $"{p.PluginVersion.Major}.{p.PluginVersion.Minor}", "N/A" };
            return rowData;
        }

        public static object[] GetPluginRowData(IMinerPlugin p)
        {
            object[] rowData = { p.PluginUUID, p.Name, "N/A", $"{p.Version.Major}.{p.Version.Minor}" };
            return rowData;
        }
    }
}
