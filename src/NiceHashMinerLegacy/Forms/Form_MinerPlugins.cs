using MinerPlugin;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

            richTextBox1.ReadOnly = true;
            richTextBox1.DetectUrls = true;
            richTextBox1.HideSelection = true;
            richTextBox1.LinkClicked += (s, e) => Process.Start(e.LinkText);

            this.Shown += new EventHandler(this.FormShown);
        }

        ~Form_MinerPlugins()
        {
            int byebye5 = 5;
        }

        private void FormShown(object sender, EventArgs e)
        {



            //foreach (var plugin in MinerPluginsManager.OnlinePlugins ?? Enumerable.Empty<PluginPackageInfo>())
            //{
            //    dataGridView1.Rows.Add(GetPluginRowData(plugin));

            //    var newRow = dataGridView1.Rows[dataGridView1.Rows.Count - 1];
            //    newRow.Tag = plugin;
            //}

            foreach (var kvp in MinerPluginsManager.MinerPlugin)
            {
                var plugin = kvp.Value;
                var pluginInfoItem = new PluginInfoItem()
                {
                    PluginName = plugin.Name,
                    PluginVersion = $"{plugin.Version.Major}.{plugin.Version.Minor}",
                    PluginAuthor = "Plugin Author build",//plugin.
                    OnPluginInfoItemMouseClick = OnPluginInfoItemMouseClick,
                    OnPluginInfoItemButtonClick = OnPluginInfoItemButtonClick,
                };
                pluginInfoItem.Tag = plugin;
                flowLayoutPanelPluginsLV.Controls.Add(pluginInfoItem);
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

        private void OnPluginInfoItemMouseClick(object sender, string value)
        {
            groupBox2.Text = value;
            richTextBox1.Text = $"OnPluginInfoItemMouseClick {value}";
        }

        private void OnPluginInfoItemButtonClick(object sender, string value)
        {
            groupBox2.Text = value;
            richTextBox1.Text = $"OnPluginInfoItemButtonClick {value}";
        }
    }
}
