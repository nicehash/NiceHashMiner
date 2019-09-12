using NiceHashMiner.Forms.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NHMCore.Mining.Plugins;
using static NHMCore.Translations;

namespace NiceHashMiner.Forms
{
    public partial class Form_MinerPlugins : Form
    {
        private Dictionary<string, PluginControlPair> _pluginInfoDetailControls = new Dictionary<string, PluginControlPair>();

        public Form_MinerPlugins()
        {
            InitializeComponent();
            flowLayoutPanelPluginsLV.Controls.Clear();
            CenterToScreen();
            Icon = NHMCore.Properties.Resources.logo;
            TopMost = NHMCore.Configs.ConfigManager.GeneralConfig.GUIWindowsAlwaysOnTop;
            FormHelpers.TranslateFormControls(this);

            Shown += new EventHandler(FormShown);
            FormClosing += new FormClosingEventHandler(Form_MinerPlugins_FormClosing);
        }        

        private void Form_MinerPlugins_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var kvp in _pluginInfoDetailControls)
            {
                var pluginUUID = kvp.Key;
                var pluginControlPair = kvp.Value;
                MinerPluginsManager.InstallRemoveProgress(pluginUUID, pluginControlPair.Progress);
            }
        }

        private class PluginControlPair
        {
            public PluginInfoItem Item;
            public PluginInfoDetails Details;
            public IProgress<Tuple<PluginInstallProgressState, int>> Progress;
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
                if (!_pluginInfoDetailControls.ContainsKey(plugin.PluginUUID)) return;
                var detailsPanel = _pluginInfoDetailControls[plugin.PluginUUID].Details;
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
            pluginInfoDetails.PluginVersion = Tr("Plugin Version: {0}", plugin.PluginVersion);
            pluginInfoDetails.PluginAuthor = Tr("Plugin Author: {0}", plugin.PluginAuthor);
            pluginInfoDetails.Description = Tr("Plugin Description: {0}", plugin.PluginDescription);
            var supportedDevs = plugin.SupportedDevicesAlgorithms
                .Where(kvp => kvp.Value.Count > 0)
                .OrderBy(kvp => kvp.Key);

            var supportedDevices = Tr("Supported Devices: {0}", string.Join(", ", supportedDevs.Select(kvp => kvp.Key)));
            pluginInfoDetails.SupportedDevices = supportedDevices;

            var supportedDevicesAlgos = supportedDevs.Select(kvp => {
                var deviceType = $"\t{kvp.Key}:";
                var algorithms = kvp.Value.Select(algo => $"\t\t- {algo}");

                var ret = deviceType + Environment.NewLine + string.Join(Environment.NewLine, algorithms);
                return ret;
            });
            var supportedDevicesAlgorithms = string.Join(Environment.NewLine, supportedDevicesAlgos).Replace("\t", "    ");

            pluginInfoDetails.SupportedDevicesAlgorithms = Tr("Supported Devices Algorithms:{0}", Environment.NewLine+supportedDevicesAlgorithms);

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

            //// update existing that are not in a task
            //var ignoreActive = GetActiveTasks();
            //foreach (var controlsPair in _pluginInfoDetails.Values)
            //{
            //    var uuid = controlsPair.Item.PluginUUID;
            //    if (ignoreActive.Contains(uuid)) continue;
            //    var plugin = MinerPluginsManager.Plugins[uuid];
            //    setPluginInfoItem(controlsPair.Item, plugin);
            //    setPluginInfoDetails(controlsPair.Details, plugin);
            //}

            var newPlugins = MinerPluginsManager.RankedPlugins.Where(plugin => _pluginInfoDetailControls.ContainsKey(plugin.PluginUUID) == false).ToList();
            var lastSingleItemRow = rankedUUIDs.Count % 2 == 1;

            // create and add new plugins
            foreach (var plugin in newPlugins)
            {
                var controlsPair = new PluginControlPair
                {
                    Item = new PluginInfoItem(),
                    Details = CreatePluginInfoDetails()
                };
                controlsPair.Item.OnButtonCancel = (s1, e1) => MinerPluginsManager.TryCancelInstall(plugin.PluginUUID);
                controlsPair.Details.OnButtonCancel = (s1, e1) => MinerPluginsManager.TryCancelInstall(plugin.PluginUUID);
                controlsPair.Progress = CreateProgressForPluginControlPair(controlsPair);
                // add control pairs
                _pluginInfoDetailControls.Add(plugin.PluginUUID, controlsPair);
                Controls.Add(controlsPair.Details);
                
                setPluginInfoItem(controlsPair.Item, plugin);
                setPluginInfoDetails(controlsPair.Details, plugin);
                // add append if any exists
                MinerPluginsManager.InstallAddProgress(plugin.PluginUUID, controlsPair.Progress);
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

                var controlsPair = _pluginInfoDetailControls[uuid];

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
            if (_pluginInfoDetailControls.ContainsKey(pluginUUID) == false) return;
            var pluginInfoControlsPair =  _pluginInfoDetailControls[pluginUUID];

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
            if (_pluginInfoDetailControls.ContainsKey(pluginUUID) == false) return;
            var pluginInfoControlsPair = _pluginInfoDetailControls[pluginUUID];
            
            await InstallOrUpdateAsync(pluginInfoControlsPair, pluginUUID);
        }

        private IProgress<Tuple<PluginInstallProgressState, int>> CreateProgressForPluginControlPair(PluginControlPair pluginInfoControlsPair)
        {
            var pluginInfoItem = pluginInfoControlsPair.Item;
            var pluginInfoDetails = pluginInfoControlsPair.Details;

            var downloadAndInstallUpdate = new Progress<Tuple<PluginInstallProgressState, int>>(statePerc =>
            {
                var state = statePerc.Item1;
                var progress = statePerc.Item2;
                var statusText = "";
                switch (state)
                {
                    case PluginInstallProgressState.Pending:
                        statusText = Tr("Pending Install");
                        break;
                    case PluginInstallProgressState.DownloadingMiner:
                        statusText = Tr("Downloading Miner: {0} %", $"{progress:F2}");
                        break;
                    case PluginInstallProgressState.DownloadingPlugin:
                        statusText = Tr("Downloading Plugin: {0} %", $"{progress:F2}");
                        break;
                    case PluginInstallProgressState.ExtractingMiner:
                        statusText = Tr("Extracting Miner: {0} %", $"{progress:F2}");
                        break;
                    case PluginInstallProgressState.ExtractingPlugin:
                        statusText = Tr("Extracting Plugin: {0} %", $"{progress:F2}");
                        break;
                    default:
                        statusText = Tr("Pending Install");
                        break;
                }

                pluginInfoItem.StatusText = statusText;
                pluginInfoItem.ProgressBarValue = progress;

                pluginInfoDetails.StatusText = statusText;
                pluginInfoDetails.ProgressBarValue = progress;

                var installing = state < PluginInstallProgressState.FailedDownloadingPlugin;
                pluginInfoItem.SwapInstallRemoveButtonWithCancelButton(installing);
                pluginInfoItem.ProgressBarVisible = installing;
                pluginInfoItem.StatusVisible = installing;

                pluginInfoDetails.SwapInstallRemoveButtonWithCancelButton(installing);
                pluginInfoDetails.ProgressBarVisible = installing;
                pluginInfoDetails.StatusVisible = installing;

                if (state == PluginInstallProgressState.Success)
                {
                    var pluginPackageInfo = MinerPluginsManager.Plugins[pluginInfoItem.PluginUUID];
                    setPluginInfoItem(pluginInfoItem, pluginPackageInfo);
                    setPluginInfoDetails(pluginInfoDetails, pluginPackageInfo);
                }
            });
            return downloadAndInstallUpdate;
        }

        private async Task InstallOrUpdateAsync(PluginControlPair pluginInfoControlsPair, string pluginUUID)
        {
            await MinerPluginsManager.DownloadAndInstall(pluginUUID, pluginInfoControlsPair.Progress);
        }
    }
}
