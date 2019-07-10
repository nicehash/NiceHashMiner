using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using NHM.Common.Enums;
using NHM.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    /// <summary>
    /// Displays devices with hashrates/profits and optional power/diag columns. During mining groups devices by miner/algo combo.
    /// </summary>
    public partial class DevicesListViewSpeedControl : DevicesListViewEnableControl, IRatesComunication
    {
        private enum Column : int
        {
            Name = 0,
            Speeds,
            Profit,
            Fiat,
            PowerUsage,
            PowerCost,
            PowerProfit,
        }

        private const string PowerKey = "power";
        private const string DiagKey = "diag";

        private const string DefaultKey = "default";

        private List<ComputeDevice> _devices;

        private readonly List<HashSet<string>> _deviceUuidsGroups = new List<HashSet<string>>();

        private readonly Timer _diagTimer = new Timer();

        private bool _ignoreChecks = false;

        public DevicesListViewSpeedControl()
        {
            InitializeComponent();

            SaveToGeneralConfig = false;

            _diagTimer.Interval = 2000;
            _diagTimer.Tick += DiagTimerOnTick;
        }

        private static bool ShowPowerCols
        {
            get => ConfigManager.GeneralConfig.ShowPowerColumns;
            set => ConfigManager.GeneralConfig.ShowPowerColumns = value;
        }

        private static bool ShowDiagCols
        {
            get => ConfigManager.GeneralConfig.ShowDiagColumns;
            set => ConfigManager.GeneralConfig.ShowDiagColumns = value;
        }

        public void SetIsMining(bool isMining)
        {
            _ignoreChecks = true;
            listViewDevices.CheckBoxes = !isMining;
            Enabled = !isMining;
            _ignoreChecks = false;
        }

        public override void InitLocale()
        {
            base.InitLocale();

            speedHeader.Text = Translations.Tr("Speeds");
        }

        protected override void ListViewDevicesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_ignoreChecks || MiningState.Instance.IsCurrentlyMining) return;
            base.ListViewDevicesItemChecked(sender, e);
        }

#region ListView updating

        public override void SetComputeDevices(List<ComputeDevice> devices)
        {
            _devices = devices;
            UpdateListView();

            if (!ShowDiagCols) return;
            _diagTimer.Start();
        }

        private void UpdateListView()
        {
            SaveToGeneralConfig = false;
            listViewDevices.BeginUpdate();
            listViewDevices.Items.Clear();

            var numItems = 4;

            RemoveOptionalHeaders(PowerKey);
            RemoveOptionalHeaders(DiagKey);
            if (ShowPowerCols)
            {
                // We always want power cols to be before diag, since they change the context of existing profit headers
                SetOptionalHeaders(PowerKey);
                numItems += 3;
            }

            if (ShowDiagCols)
            { 
                SetOptionalHeaders(DiagKey);
                numItems += 3;
            }

            // set devices
            var allIndices = _deviceUuidsGroups.SelectMany(i => i).ToList();
            var inactiveIndices = new List<string>();
            foreach (var computeDevice in _devices)
            {
                var lvi = new ListViewItem
                {
                    Checked = computeDevice.Enabled,
                    Text = computeDevice.GetFullName(),
                    Tag = computeDevice
                };

                for (var i = 0; i < numItems; i++)
                {
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                }
                
                listViewDevices.Items.Add(lvi);

                if (!allIndices.Contains(computeDevice.Uuid))
                    inactiveIndices.Add(computeDevice.Uuid);
            }

            foreach (var group in listViewDevices.Groups)
            {
                if (!(group is ListViewGroup g)) continue;

                if (g.Tag is string groupKey)
                {
                    foreach (var lvi in listViewDevices.Items)
                    {
                        if (lvi is ListViewItem item && item.Tag is ComputeDevice dev && groupKey.Contains(dev.Uuid))
                        {
                            g.Items.Add(item);
                        }
                    }

                    if (g.Items.Count <= 0) continue;

                    var t = SetTotalRow(groupKey, numItems);
                    g.Items.Add(t);
                }
                else if (g.Name == DefaultKey)
                {
                    foreach (var lvi in listViewDevices.Items)
                    {
                        if (lvi is ListViewItem item && item.Tag is ComputeDevice dev &&
                            inactiveIndices.Contains(dev.Uuid))
                        {
                            g.Items.Add(item);
                        }
                    }
                }
            }

            listViewDevices.EndUpdate();
            listViewDevices.Invalidate(true);
            // reset properties
            SaveToGeneralConfig = true;
        }

        private ListViewItem SetTotalRow(string groupKey, int numSubs)
        {
            var total = new ListViewItem
            {
                Text = Translations.Tr("Total"),
                Tag = groupKey
            };
            for (var i = 0; i < numSubs; i++)
            {
                total.SubItems.Add(new ListViewItem.ListViewSubItem());
            }
            listViewDevices.Items.Add(total);
            return total;
        }

        protected override void SetLvi(ListViewItem lvi, int index)
        {
            foreach (var group in listViewDevices.Groups)
            {
                if (group is ListViewGroup g && g.Tag is List<int> indices &&
                    indices.Contains(index) && !g.Items.Contains(lvi))
                {
                    g.Items.Add(lvi);
                }
            }
        }

        public void SetPayingColumns()
        {
            profitHeader.Text = FormatPerTimeUnit("mBTC");
            fiatHeader.Text = CurrencyPerTimeUnit();
        }

        protected override void DevicesListViewEnableControl_Resize(object sender, EventArgs e)
        {
        }

#endregion

#region Optional Headers

        protected override void ListViewDevices_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            contextMenuStrip1.Items.Clear();

            var showPower = new ToolStripMenuItem("Show Power Info")
            {
                Tag = PowerKey,
                Checked = ShowPowerCols
            };
            var showDiag = new ToolStripMenuItem("Show Diagnostic Info")
            {
                Tag = DiagKey,
                Checked = ShowDiagCols
            };

            showPower.Click += SetPowerHeaders;
            showDiag.Click += SetDiagHeaders;

            contextMenuStrip1.Items.Add(showPower);
            contextMenuStrip1.Items.Add(showDiag);
            contextMenuStrip1.Show(Cursor.Position);
        }

        private void SetPowerHeaders(object sender, EventArgs e)
        {
            ShowPowerCols = !ShowPowerCols;
            UpdateListView();
        }

        private void SetDiagHeaders(object sender, EventArgs e)
        {
            ShowDiagCols = !ShowDiagCols;
            UpdateListView();

            if (ShowDiagCols)
            {
                _diagTimer.Start();
            }
            else
            {
                _diagTimer.Stop();
            }
        }

        private void DiagTimerOnTick(object sender, EventArgs e)
        {
            foreach (var lvi in listViewDevices.Items)
            {
                if (!(lvi is ListViewItem item) || !(item.Tag is ComputeDevice dev)) continue;
                // indexes are all off by 1 fix these indexes
                SetDiagText(item, 0 - 1, (int) dev.Load);
                SetDiagText(item, 1 - 1, (int) dev.Temp);
                SetDiagText(item, 2 - 1, dev.FanSpeed);
            }
        }

        private static void SetDiagText(ListViewItem item, int index, int value)
        {
            if (value < 0) return;

            var start = ShowPowerCols ? 8 : 5;
            if (item.SubItems.Count <= start + index) return;
            item.SubItems[start + index].Text = value.ToString();
        }

        private static void SetPowerText(ListViewItem lvi, double powerUsage, double powerCost, double? profit)
        {
            if (!ShowPowerCols) return;
            if (lvi.SubItems.Count <= 7) return;

            lvi.SubItems[(int)Column.PowerUsage].Text = powerUsage.ToString("F2");
            lvi.SubItems[(int)Column.PowerCost].Text = powerCost.ToString("F2");
            lvi.SubItems[(int)Column.PowerProfit].Text = profit?.ToString("F2") ?? "";
        }

        private void SetOptionalHeaders(string key)
        {
            for (var i = 0; i < 3; i++)
            {
                if (listViewDevices.Columns.ContainsKey($"{key}{i}"))
                    continue;

                listViewDevices.Columns.Add($"{key}{i}", GetHeaderName(key, i), 60, HorizontalAlignment.Right, "");
            }
        }

        private void RemoveOptionalHeaders(string key)
        {
            for (var i = 0; i < 3; i++)
            {
                listViewDevices.Columns.RemoveByKey($"{key}{i}");
            }
        }

        private static string GetHeaderName(string key, int index)
        {
            var langKey = "";
            if (key == PowerKey)
            {
                switch (index)
                {
                    case 0:
                        langKey = "Power Usage (W)";
                        break;
                    case 1:
                        return Translations.Tr("Power Cost ({0})", CurrencyPerTimeUnit());
                    case 2:
                        return Translations.Tr("Profit ({0})", CurrencyPerTimeUnit());
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        langKey = "Load (%)";
                        break;
                    case 1:
                        langKey = "Temp (C)";
                        break;
                    case 2:
                        langKey = "RPM";
                        break;
                }
            }

            return Translations.Tr(langKey);
        }

#endregion

#region IRatesCommunication Implementation

        public void ClearRatesAll()
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                _deviceUuidsGroups.Clear();
                listViewDevices.Groups.Clear();
                UpdateListView();
            });
        }

        private void refreshRateInfoGui()
        {
            Enabled = true;

            // Ensure we have disabled group
            if (listViewDevices.Groups[DefaultKey] == null)
            {
                _ignoreChecks = true;  // For some reason without this it will enable some checkboxes
                listViewDevices.Groups.Clear();
                var disGrp = new ListViewGroup(DefaultKey, Translations.Tr("Disabled"));
                listViewDevices.Groups.Add(disGrp);
                _ignoreChecks = false;
            }

            var kwhPriceInBtc = ExchangeRateApi.GetKwhPriceInBtc();
            var minersMiningStats = MiningStats.GetMinersMiningStats();
            var devicesMiningStats = MiningStats.GetDevicesMiningStats();
            foreach(var minerStats in minersMiningStats)
            {
                // get data
                var algorithmFirstType = minerStats.Speeds.Count > 0 ? minerStats.Speeds[0].type : AlgorithmType.NONE;
                var algorithmSecondType = minerStats.Speeds.Count > 1 ? minerStats.Speeds[1].type : AlgorithmType.NONE;
                var algorithmName = Helpers.GetNameFromAlgorithmTypes(algorithmFirstType, algorithmSecondType);
                var minerName = minerStats.MinerName;
                var name = minerName != "" ? $"{algorithmName} ({minerName})" : algorithmName;
                var firstSpeed = minerStats.Speeds.Count > 0 ? minerStats.Speeds[0].speed : 0d;
                var secondSpeed = minerStats.Speeds.Count > 1 ? minerStats.Speeds[1].speed : 0d;

                // ID for algo/miner combo
                var key = minerStats.GroupKey;
                // If index is not in any of the groups of indices
                if (!_deviceUuidsGroups.Any(l => minerStats.DeviceUUIDs.Except(l).Count() == 0))
                {
                    _deviceUuidsGroups.Add(minerStats.DeviceUUIDs);
                }
                // Make group for this algo/miner combo if not made already
                if (listViewDevices.Groups[key] == null)
                {
                    var group = new ListViewGroup(key, name)
                    {
                        Tag = minerStats.GroupKey
                    };
                    listViewDevices.Groups.Add(group);

                    UpdateListView();
                }

                foreach (var lvi in listViewDevices.Items)
                {
                    if (!(lvi is ListViewItem item)) continue;

                    if (item.Tag is string groupKey && groupKey == minerStats.GroupKey)
                    {
                        // This is a total row
                        var minerRevenue = minerStats.TotalPayingRate();
                        var minerProfit = minerStats.TotalPayingRateDeductPowerCost(kwhPriceInBtc);
                        UpdateRowInfo(item, firstSpeed, secondSpeed, algorithmFirstType, minerRevenue, minerProfit, minerStats.PowerCost(kwhPriceInBtc), minerStats.GetPowerUsage());
                    }
                    else if (item.Tag is ComputeDevice dev && minerStats.DeviceUUIDs.Any(uuid => uuid == dev.Uuid))
                    {
                        var deviceStat = devicesMiningStats.Where(stat => stat.DeviceUUID == dev.Uuid).FirstOrDefault();
                        if (deviceStat == null) continue;
                        // This is a dev row
                        var firstDeviceSpeed = deviceStat.Speeds.Count > 0 ? deviceStat.Speeds[0].speed : 0d;
                        var secondtDeviceSpeed = deviceStat.Speeds.Count > 1 ? deviceStat.Speeds[1].speed : 0d;
                        var deviceRevenue = deviceStat.TotalPayingRate();
                        var deviceProfit = deviceStat.TotalPayingRateDeductPowerCost(kwhPriceInBtc);
                        UpdateRowInfo(item, firstDeviceSpeed, secondtDeviceSpeed, algorithmFirstType, deviceRevenue, deviceProfit, deviceStat.PowerCost(kwhPriceInBtc), deviceStat.GetPowerUsage());
                    }
                }
            }
        }
        
        public void RefreshRates()
        {
            FormHelpers.SafeInvoke(this, () => {
                refreshRateInfoGui();
            });
        }

        #endregion

        #region Helpers

        private static string CurrencyPerTimeUnit()
        {
            return FormatPerTimeUnit(ExchangeRateApi.ActiveDisplayCurrency);
        }

        private static string FormatPerTimeUnit(string unit)
        {
            var timeUnit = Translations.Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
            return $"{unit}/{timeUnit}";
        }
        
        private static void UpdateRowInfo(ListViewItem item, double speed, double secSpeed, AlgorithmType algorithmType, double revenue, double profit,
            double power, double powerUsage)
        {
            try
            {
                item.SubItems[(int)Column.Speeds].Text = Helpers.FormatDualSpeedOutput(speed, secSpeed, algorithmType);
                //item.SubItems[SecSpeed].Text = secSpeed > 0 ? Helpers.FormatSpeedOutput(secSpeed) : "";

                var fiat = ExchangeRateApi.ConvertFromBtc(profit * TimeFactor.TimeUnit);
                if (ShowPowerCols)
                {
                    // When showing power cols, the default "profit" header changes to revenue
                    // The power headers then explain cost of power and real profit after subtracting this
                    item.SubItems[(int)Column.Profit].Text = (revenue * 1000 * TimeFactor.TimeUnit).ToString("F4");
                    item.SubItems[(int)Column.Fiat].Text = ExchangeRateApi.ConvertFromBtc(revenue * TimeFactor.TimeUnit).ToString("F2");
                    var powerCost = ExchangeRateApi.ConvertFromBtc(power * TimeFactor.TimeUnit);
                    SetPowerText(item, powerUsage, powerCost, fiat);
                }
                else
                {
                    item.SubItems[(int)Column.Profit].Text = (profit * 1000 * TimeFactor.TimeUnit).ToString("F4");
                    item.SubItems[(int)Column.Fiat].Text = fiat.ToString("F2");
                }
            }
            catch { }
        }

        #endregion
    }
}
