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
using NiceHashMiner.Forms;
using System.Net;
using System.Threading;

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

        private static string PluginInstallRemoveText(bool installed)
        {
            return installed ? Tr("Remove Plugin") : Tr("Install Plugin");
        }

        private void FormShown(object sender, EventArgs e)
        {
            foreach (var kvp in MinerPluginsManager.Plugins)
            {
                var plugin = kvp.Value;
                var pluginInfoItem = new PluginInfoItem()
                {
                    PluginUUID = plugin.PluginUUID,
                    PluginName = plugin.PluginName,
                    PluginVersion = Tr("Version: {0}", $"{plugin.PluginVersion.Major}.{plugin.PluginVersion.Minor}"),
                    PluginAuthor = Tr("Author: {0}", plugin.PluginAuthor),
                    ButtonInstallRemoveText = PluginInstallRemoveText(plugin.Installed),
                    ButtonUpdateEnabled = plugin.HasNewerVersion,
                    OnPluginInfoItemMouseClick = OnPluginInfoItemMouseClick,
                    OnButtonInstallRemoveClick = OnButtonInstallRemoveClick,
                    OnButtonUpdateClick = OnButtonUpdateClick,
                };
                pluginInfoItem.Tag = plugin;
                flowLayoutPanelPluginsLV.Controls.Add(pluginInfoItem);
            }
        }

        private void OnPluginInfoItemMouseClick(object sender, string pluginUUID)
        {
            //var pluginInfoItem = sender as PluginInfoItem;
            //if (pluginInfoItem != null)
            //{
            //    pluginInfoItem.BackColor = Color.LightGray;
            //}

            var plugin = MinerPluginsManager.Plugins[pluginUUID];
            groupBox2.Text = plugin.PluginName;
            richTextBox1.Text = $"";
            richTextBox1.Text += $"PluginName: {plugin.PluginName}" + Environment.NewLine;
            richTextBox1.Text += $"PluginVersion: {plugin.PluginVersion}" + Environment.NewLine;
            richTextBox1.Text += $"PluginAuthor: {plugin.PluginAuthor}" + Environment.NewLine;
            richTextBox1.Text += $"PluginDescription: {plugin.PluginDescription}" + Environment.NewLine;
            richTextBox1.Text += $"Installed: {plugin.Installed}" + Environment.NewLine;
            richTextBox1.Text += $"HasNewerVersion: {plugin.HasNewerVersion}" + Environment.NewLine;
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

        private async void OnButtonInstallRemoveClick(object sender, string pluginUUID)
        {
            var pluginInfoItem = sender as PluginInfoItem;
            if (pluginInfoItem == null) return;

            var pluginPackageInfo = MinerPluginsManager.Plugins[pluginUUID];
            var oldUpdateButtonEnabledValue = pluginInfoItem.ButtonUpdateEnabled;
            try
            {
                pluginInfoItem.ButtonInstallRemoveEnabled = false;
                pluginInfoItem.ButtonUpdateEnabled = false;

                var actionText = pluginPackageInfo.Installed ? "Upgrade" : "Install";
                groupBox2.Text = pluginPackageInfo.PluginName;
                richTextBox1.Text = $"OnPluginInfoItemButtonClick {actionText}";

                // remove if installed
                if (pluginPackageInfo.Installed)
                {
                    MinerPluginsManager.Remove(pluginUUID);
                    //flowLayoutPanelPluginsLV.Controls.Remove(pluginInfoItem);
                    pluginInfoItem.StatusText = "Removed";
                }
                else if (pluginPackageInfo.Installed == false)
                {
                    var cancelInstall = new CancellationTokenSource();
                    MinerPluginsManager.DownloadAndInstallUpdate downloadAndInstallUpdate = (string infoStr) => {
                        FormHelpers.SafeInvoke(pluginInfoItem, () => { pluginInfoItem.StatusText = infoStr; });
                    };
                    await MinerPluginsManager.DownloadAndInstall(pluginPackageInfo, downloadAndInstallUpdate, cancelInstall.Token);
                }

            }
            catch (Exception e)
            {
            }
            finally
            {
                pluginInfoItem.ButtonInstallRemoveText = PluginInstallRemoveText(pluginPackageInfo.Installed);
                pluginInfoItem.ButtonInstallRemoveEnabled = true;
                pluginInfoItem.ButtonUpdateEnabled = oldUpdateButtonEnabledValue;
            }
        }

        private async void OnButtonUpdateClick(object sender, string pluginUUID)
        {
            var pluginInfoItem = sender as PluginInfoItem;
            if (pluginInfoItem == null) return;

            var pluginPackageInfo = MinerPluginsManager.Plugins[pluginUUID];
            var oldUpdateButtonEnabledValue = pluginInfoItem.ButtonUpdateEnabled;
            try
            {
                pluginInfoItem.ButtonInstallRemoveEnabled = false;
                pluginInfoItem.ButtonUpdateEnabled = false;

                groupBox2.Text = pluginPackageInfo.PluginName;
                richTextBox1.Text = $"OnButtonUpdateClick";

                // update
                var cancelInstall = new CancellationTokenSource();
                MinerPluginsManager.DownloadAndInstallUpdate downloadAndInstallUpdate = (string infoStr) =>
                {
                    FormHelpers.SafeInvoke(pluginInfoItem, () => { pluginInfoItem.StatusText = infoStr; });
                };
                await MinerPluginsManager.DownloadAndInstall(pluginPackageInfo, downloadAndInstallUpdate, cancelInstall.Token);
                var ver = pluginPackageInfo.PluginVersion;
                pluginInfoItem.PluginVersion = Tr("Version: {0}", $"{ver.Major}.{ver.Minor}");
            }
            catch (Exception e)
            {
            }
            finally
            {
                pluginInfoItem.ButtonInstallRemoveText = PluginInstallRemoveText(pluginPackageInfo.Installed);
                pluginInfoItem.ButtonInstallRemoveEnabled = true;
            }
        }
    }
}
