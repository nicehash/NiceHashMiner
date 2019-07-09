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
using static NiceHashMiner.Plugin.MinerPluginsManager;
using NiceHashMiner.Configs;

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
            this.TopMost = ConfigManager.GeneralConfig.GUIWindowsAlwaysOnTop;
            FormHelpers.TranslateFormControls(this);

            Shown += new EventHandler(FormShown);
            FormClosing += new FormClosingEventHandler(Form_MinerPlugins_FormClosing);
        }        

        private void Form_MinerPlugins_FormClosing(object sender, FormClosingEventArgs e)
        {
            var details = _pluginInfoDetails.Values.Select(pair => pair.Details).Where(detail => detail != null);
            foreach (var pluginInfoDetails in details)
            {
                pluginInfoDetails.Visible = false;
            }
        }

        //private PluginInfoDetails pluginInfoDetails1;
        // save references to details panels

        private class PluginControlPair
        {
            public PluginInfoItem Item;
            public PluginInfoDetails Details;
        }

        private Dictionary<string, PluginControlPair> _pluginInfoDetails = new Dictionary<string, PluginControlPair>();

        private object _lock = new object();
        private HashSet<string> _activeTasks = new HashSet<string>();
        
        private void AddActiveTask(string uuid)
        {
            lock(_lock)
            {
                _activeTasks.Add(uuid);
            }
        }

        private void RemoveActiveTask(string uuid)
        {
            lock (_lock)
            {
                _activeTasks.Remove(uuid);
            }
        }

        private List<string> GetActiveTasks()
        {
            var ret = new List<string>();
            lock (_lock)
            {
                foreach (var uuid in _activeTasks) ret.Add(uuid);
            }
            return ret;
        }


        private static string PluginInstallRemoveText(PluginPackageInfoCR plugin)
        {
            if (plugin.Installed) return Tr("Remove");
            if (plugin.OnlineSupportedDeviceCount > 0) return Tr("Install");
            return Tr("Not Supported");
        }

        private static bool PluginInstallRemoveEnabled(PluginPackageInfoCR plugin)
        {
            return plugin.Installed || plugin.OnlineSupportedDeviceCount > 0;
        }

        private void setPluginInfoItem(PluginInfoItem pluginInfoItem, PluginPackageInfoCR plugin)
        {
            pluginInfoItem.PluginUUID = plugin.PluginUUID;
            pluginInfoItem.Description = plugin.PluginDescription;
            pluginInfoItem.PluginName = plugin.PluginName;
            pluginInfoItem.PluginVersion = Tr("Version: {0}", $"{plugin.PluginVersion.Major}.{plugin.PluginVersion.Minor}");
            pluginInfoItem.PluginAuthor = Tr("Author: {0}", plugin.PluginAuthor);

            //pluginInfoItem.OnPluginInfoItemMouseClick = OnPluginInfoItemMouseClick;
            pluginInfoItem.OnDetailsClick = (s, e) => {
                if (!_pluginInfoDetails.ContainsKey(plugin.PluginUUID)) return;
                var detailsPanel = _pluginInfoDetails[plugin.PluginUUID].Details;
                if (detailsPanel == null) return;
                detailsPanel.BringToFront();
                detailsPanel.Visible = true;
            };
            pluginInfoItem.ButtonInstallRemoveText = PluginInstallRemoveText(plugin);
            pluginInfoItem.ButtonInstallRemoveEnabled = PluginInstallRemoveEnabled(plugin);
            pluginInfoItem.ButtonUpdateVisible = plugin.HasNewerVersion;
            pluginInfoItem.OnButtonInstallRemoveClick = OnButtonInstallRemoveClick;
            pluginInfoItem.OnButtonUpdateClick = OnButtonUpdateClick;
            // TODO maybe we would want a visible status
            pluginInfoItem.StatusVisible = false;
            FormHelpers.TranslateFormControls(pluginInfoItem);
        }

        private void setPluginInfoDetails(PluginInfoDetails pluginInfoDetails, PluginPackageInfoCR plugin)
        {
            pluginInfoDetails.PluginUUID = plugin.PluginUUID;
            pluginInfoDetails.PluginName = plugin.PluginName;
            pluginInfoDetails.PluginVersion = $"Plugin Version: {plugin.PluginVersion}";
            pluginInfoDetails.PluginAuthor = $"Plugin Author: {plugin.PluginAuthor}";
            pluginInfoDetails.Description = $"Plugin Description: {plugin.PluginDescription}";
            var supportedDevs = plugin.SupportedDevicesAlgorithms
                .Where(kvp => kvp.Value.Count > 0)
                .OrderBy(kvp => kvp.Key);

            var supportedDevices = $"Supported Devices: {string.Join(",", supportedDevs.Select(kvp => kvp.Key))}";
            pluginInfoDetails.SupportedDevices = supportedDevices;

            var supportedDevicesAlgos = supportedDevs.Select(kvp => {
                var deviceType = $"\t{kvp.Key}:";
                var algorithms = kvp.Value.Select(algo => $"\t\t- {algo}");

                var ret = deviceType + Environment.NewLine + string.Join(Environment.NewLine, algorithms);
                return ret;
            });
            var supportedDevicesAlgorithms = string.Join(Environment.NewLine, supportedDevicesAlgos).Replace("\t", "    ");

            pluginInfoDetails.SupportedDevicesAlgorithms = $"Supported Devices Algorithms:{Environment.NewLine}{supportedDevicesAlgorithms}";

            pluginInfoDetails.StatusText = "";

            pluginInfoDetails.ButtonInstallRemoveText = PluginInstallRemoveText(plugin);
            pluginInfoDetails.ButtonInstallRemoveEnabled = PluginInstallRemoveEnabled(plugin);
            pluginInfoDetails.ButtonUpdateVisible = plugin.HasNewerVersion;
            pluginInfoDetails.OnButtonInstallRemoveClick = OnButtonInstallRemoveClick;
            pluginInfoDetails.OnButtonUpdateClick = OnButtonUpdateClick;
            FormHelpers.TranslateFormControls(pluginInfoDetails);
        }

        private static PluginInfoDetails CreatePluginInfoDetails()
        {
            var pluginInfoDetails1 = new PluginInfoDetails()
            {
                Visible = false,
                Dock = DockStyle.Fill
            };
            pluginInfoDetails1.OnBackClick = (s, e) => pluginInfoDetails1.Visible = false;
            return pluginInfoDetails1;
        }

        private void FormShown(object sender, EventArgs e)
        {
            // TODO blocking make it async
            MinerPluginsManager.CrossReferenceInstalledWithOnline();

            var rankedUUIDs = MinerPluginsManager.RankedPlugins.Select(plugin => plugin.PluginUUID).ToList();

            // update existing that are not in a task
            var ignoreActive = GetActiveTasks();
            foreach (var controlsPair in _pluginInfoDetails.Values)
            {
                var uuid = controlsPair.Item.PluginUUID;
                if (ignoreActive.Contains(uuid)) continue;
                var plugin = MinerPluginsManager.Plugins[uuid];
                setPluginInfoItem(controlsPair.Item, plugin);
                setPluginInfoDetails(controlsPair.Details, plugin);
            }

            var newPlugins = MinerPluginsManager.RankedPlugins.Where(plugin => _pluginInfoDetails.ContainsKey(plugin.PluginUUID) == false).ToList();
            var lastSingleItemRow = rankedUUIDs.Count % 2 == 1;

            // create and add new plugins
            foreach (var plugin in newPlugins)
            {
                var controlsPair = new PluginControlPair
                {
                    Item = new PluginInfoItem(),
                    Details = CreatePluginInfoDetails()
                };
                // add control pairs
                _pluginInfoDetails.Add(plugin.PluginUUID, controlsPair);
                Controls.Add(controlsPair.Details);
                
                setPluginInfoItem(controlsPair.Item, plugin);
                setPluginInfoDetails(controlsPair.Details, plugin);
            }

            // get row count
            var rowsNeeded = rankedUUIDs.Count / 2 + rankedUUIDs.Count % 2;
            var rowsAdded = flowLayoutPanelPluginsLV.Controls.Count;

            // we have new rows
            if (rowsAdded < rowsNeeded)
            {
                for (int add = 0; add < rowsNeeded - rowsAdded; add++)
                {
                    var pluginRowItem = new PluginInfoItemRow();
                    flowLayoutPanelPluginsLV.Controls.Add(pluginRowItem);
                }
            }
            // we have too many
            else if(rowsAdded > rowsNeeded)
            {
                var toRemoveCount = rowsAdded - rowsNeeded;
                for (int remove = 0; remove < toRemoveCount; remove++)
                {
                    var lastIndex = flowLayoutPanelPluginsLV.Controls.Count - 1;
                    var lastRow = flowLayoutPanelPluginsLV.Controls[lastIndex] as PluginInfoItemRow;
                    if (lastRow != null)
                    {
                        lastRow.RemovePluginInfoItem1();
                        lastRow.RemovePluginInfoItem2();
                    }
                    flowLayoutPanelPluginsLV.Controls.RemoveAt(lastIndex);
                }
            }

            for (int item = 0; item < rankedUUIDs.Count; item++)
            {
                var uuid = rankedUUIDs[item];
                var rowIndex = (item / 2);
                var isFirst = (item % 2) == 0;

                var controlsPair = _pluginInfoDetails[uuid];

                var row = flowLayoutPanelPluginsLV.Controls[rowIndex] as PluginInfoItemRow;
                if (row == null) continue; 
                if (isFirst)
                {
                    row.SetPluginInfoItem1(controlsPair.Item);
                }
                else
                {
                    row.SetPluginInfoItem2(controlsPair.Item);
                }
            }
            if (lastSingleItemRow)
            {
                var lastIndex = flowLayoutPanelPluginsLV.Controls.Count - 1;
                var lastRow = flowLayoutPanelPluginsLV.Controls[lastIndex] as PluginInfoItemRow;
                if (lastRow != null)
                {
                    lastRow.RemovePluginInfoItem2();
                }
            }
        }

        private async void OnButtonInstallRemoveClick(object sender, string pluginUUID)
        {
            if (_pluginInfoDetails.ContainsKey(pluginUUID) == false) return;
            var pluginInfoControlsPair =  _pluginInfoDetails[pluginUUID];

            var pluginPackageInfo = MinerPluginsManager.Plugins[pluginUUID];
            try
            {
                // remove if installed
                if (pluginPackageInfo.Installed)
                {
                    MinerPluginsManager.Remove(pluginUUID);
                    //flowLayoutPanelPluginsLV.Controls.Remove(pluginInfoItem);
                    var pluginInfoItem = pluginInfoControlsPair.Item;
                    pluginInfoItem.StatusText = "Removed";
                    var pluginInfoDetails = pluginInfoControlsPair.Details;
                    pluginInfoDetails.StatusText = "";
                }
                else if (pluginPackageInfo.Installed == false)
                {
                    await InstallOrUpdateAsync(pluginInfoControlsPair, pluginUUID);
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                setPluginInfoItem(pluginInfoControlsPair.Item, pluginPackageInfo);
                setPluginInfoDetails(pluginInfoControlsPair.Details, pluginPackageInfo);
            }
        }

        private async void OnButtonUpdateClick(object sender, string pluginUUID)
        {
            if (_pluginInfoDetails.ContainsKey(pluginUUID) == false) return;
            var pluginInfoControlsPair = _pluginInfoDetails[pluginUUID];
            
            await InstallOrUpdateAsync(pluginInfoControlsPair, pluginUUID);
        }

        private async Task InstallOrUpdateAsync(PluginControlPair pluginInfoControlsPair, string pluginUUID)
        {
            var pluginInfoItem = pluginInfoControlsPair.Item;
            var pluginInfoDetails = pluginInfoControlsPair.Details;
            // update
            var cancelInstall = new CancellationTokenSource();
            var pluginPackageInfo = MinerPluginsManager.Plugins[pluginUUID];
            try
            {
                AddActiveTask(pluginUUID);
                pluginInfoItem.OnButtonCancel = (s, e) => cancelInstall.Cancel();
                pluginInfoItem.SwapInstallRemoveButtonWithCancelButton(true);
                pluginInfoItem.ProgressBarVisible = true;
                pluginInfoItem.StatusVisible = true;
                //pluginInfoItem.ButtonUpdateEnabled = false;

                pluginInfoDetails.OnButtonCancel = (s, e) => cancelInstall.Cancel();
                pluginInfoDetails.SwapInstallRemoveButtonWithCancelButton(true);
                pluginInfoDetails.ProgressBarVisible = true;
                pluginInfoDetails.StatusVisible = true;

                pluginInfoItem.StatusText = "Pending Install";
                pluginInfoDetails.StatusText = "Pending Install";


                var downloadAndInstallUpdate = new Progress<Tuple<ProgressState, int>>(statePerc  =>
                {
                    var state = statePerc.Item1;
                    var progress = statePerc.Item2;
                    var statusText = "";
                    switch (state)
                    {
                        case ProgressState.DownloadingMiner:
                            statusText = $"Downloading Miner: {progress} %";
                            break;

                        case ProgressState.DownloadingPlugin:
                            statusText = $"Downloading Plugin: {progress} %";
                            break;

                        case ProgressState.ExtractingMiner:
                            statusText = $"Extracting Miner: {progress} %";
                            break;

                        case ProgressState.ExtractingPlugin:
                            statusText = $"Extracting Plugin: {progress} %";
                            break;

                    }
                    // SafeInvoke is not needed inside a Progress 
                    //FormHelpers.SafeInvoke(pluginInfoItem, () => {
                        pluginInfoItem.StatusText = statusText;
                        pluginInfoItem.ProgressBarValue = progress;
                    //});
                    //FormHelpers.SafeInvoke(pluginInfoDetails, () => {
                        pluginInfoDetails.StatusText = statusText;
                        pluginInfoDetails.ProgressBarValue = progress;
                    //});
                });
                await MinerPluginsManager.DownloadAndInstall(pluginPackageInfo, downloadAndInstallUpdate, cancelInstall.Token);
                
            }
            catch (Exception e)
            {
            }
            finally
            {
                pluginInfoItem.ProgressBarVisible = false;
                pluginInfoDetails.ProgressBarVisible = false;
                pluginInfoItem.SwapInstallRemoveButtonWithCancelButton(false);
                pluginInfoDetails.SwapInstallRemoveButtonWithCancelButton(false);
                setPluginInfoItem(pluginInfoItem, pluginPackageInfo);
                setPluginInfoDetails(pluginInfoDetails, pluginPackageInfo);
                RemoveActiveTask(pluginUUID);
            }
        }
    }
}
