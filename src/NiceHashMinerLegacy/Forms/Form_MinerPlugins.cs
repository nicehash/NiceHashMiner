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
using static NiceHashMiner.Translations;

namespace NiceHashMiner.Forms
{
    public partial class Form_MinerPlugins : Form
    {
        public Form_MinerPlugins()
        {
            InitializeComponent();
            flowLayoutPanelPluginsLV.Controls.Clear();
            CenterToScreen();
            Icon = Properties.Resources.logo;
            FormHelpers.TranslateFormControls(this);

            richTextBox1.ReadOnly = true;
            richTextBox1.DetectUrls = true;
            richTextBox1.HideSelection = true;
            richTextBox1.LinkClicked += (s, e) => Process.Start(e.LinkText);

            this.Shown += new EventHandler(this.FormShown);
        }

        private void FormShown(object sender, EventArgs e)
        {
            foreach (var kvp in MinerPluginsManager.Plugins)
            {
                var plugin = kvp.Value;
                var buttonText = Tr("Install Plugin");
                if (plugin.Installed)
                {
                    // Well you can have a plugin that can be updated and removed so add another button
                    buttonText = Tr("Remove Plugin");
                }
                else if (plugin.Installed && plugin.LatestVersion)
                {
                    buttonText = Tr("Update Plugin");
                }
                var pluginInfoItem = new PluginInfoItem()
                {
                    PluginUUID = plugin.PluginUUID,
                    PluginName = plugin.PluginName,
                    PluginVersion = Tr("Version: {0}", $"{plugin.PluginVersion.Major}.{plugin.PluginVersion.Minor}"),
                    PluginAuthor = Tr("Author: {0}", plugin.PluginAuthor),
                    ButtonText = buttonText,
                    OnPluginInfoItemMouseClick = OnPluginInfoItemMouseClick,
                    OnPluginInfoItemButtonClick = OnPluginInfoItemButtonClick,
                };
                pluginInfoItem.Tag = plugin;
                flowLayoutPanelPluginsLV.Controls.Add(pluginInfoItem);
            }
        }

        private void OnPluginInfoItemMouseClick(object sender, string pluginUUID)
        {
            var plugin = MinerPluginsManager.Plugins[pluginUUID];
            groupBox2.Text = plugin.PluginName;
            richTextBox1.Text = $"";
            richTextBox1.Text += $"PluginName: {plugin.PluginName}" + Environment.NewLine;
            richTextBox1.Text += $"PluginVersion: {plugin.PluginVersion}" + Environment.NewLine;
            richTextBox1.Text += $"PluginAuthor: {plugin.PluginAuthor}" + Environment.NewLine;
            richTextBox1.Text += $"PluginDescription: {plugin.PluginDescription}" + Environment.NewLine;
            richTextBox1.Text += $"Installed: {plugin.Installed}" + Environment.NewLine;
            richTextBox1.Text += $"LatestVersion: {plugin.LatestVersion}" + Environment.NewLine;
            richTextBox1.Text += $"OnlineVersion: {plugin.OnlineVersion}" + Environment.NewLine;
            richTextBox1.Text += $"PluginUUID: {plugin.PluginUUID}" + Environment.NewLine;
            richTextBox1.Text += $"PluginPackageURL: {plugin.PluginPackageURL}" + Environment.NewLine;
            richTextBox1.Text += $"MinerPackageURL: {plugin.MinerPackageURL}" + Environment.NewLine;
            var supportedDevsAlgos = "";
            if (plugin.SupportedDevicesAlgorithms != null)
            {
                foreach (var devAlgos in plugin.SupportedDevicesAlgorithms)
                {
                    supportedDevsAlgos += $"{devAlgos.Key}:" + Environment.NewLine;
                    foreach (var algo in devAlgos.Value)
                    {
                        supportedDevsAlgos += $"\t - {algo}," + Environment.NewLine;
                    }
                }
            }
            richTextBox1.Text += $"SupportedDevicesAlgorithms: {supportedDevsAlgos}" + Environment.NewLine;
        }

        private void OnPluginInfoItemButtonClick(object sender, string pluginUUID)
        {
            var plugin = MinerPluginsManager.Plugins[pluginUUID];
            var actionText = plugin.Installed ? "Upgrade" : "Install";
            groupBox2.Text = plugin.PluginName;
            richTextBox1.Text = $"OnPluginInfoItemButtonClick {actionText}";
            // remove if installed
            if (plugin.Installed)
            {
                MinerPluginsManager.Remove(pluginUUID);
                // find and remove
                var removeAtIndex = -1;
                for (int i = 0; i < flowLayoutPanelPluginsLV.Controls.Count; ++i)
                {
                    var c = flowLayoutPanelPluginsLV.Controls[i] as PluginInfoItem;
                    if (c != null && c.PluginUUID == pluginUUID)
                    {
                        removeAtIndex = i;
                        // TODO this might fail because we are inside a for?
                        flowLayoutPanelPluginsLV.Controls.RemoveAt(removeAtIndex);
                        break;
                    }
                }
            }
        }
    }
}
