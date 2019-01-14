using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    /// <summary>
    /// Displays devices with hashrates/profits and optional power/diag columns. During mining groups devices by miner/algo combo.
    /// </summary>
    internal partial class DevicesListViewSpeedControl : DevicesListViewEnableControl, IRatesComunication
    {
        private const int Speed = 1;
        private const int SecSpeed = 2;
        private const int Profit = 3;
        private const int Fiat = 4;
        private const int PowerUsage = 5;
        private const int PowerCost = 6;
        private const int PowerProfit = 7;

        private const string PowerKey = "power";
        private const string DiagKey = "diag";

        private const string DefaultKey = "default";

        private List<ComputeDevice> _devices;

        private readonly List<List<int>> _indexTotals = new List<List<int>>();

        public IGlobalRatesUpdate GlobalRates;

        private readonly Timer _diagTimer = new Timer();

        private bool _ignoreChecks = false;

        public DevicesListViewSpeedControl()
        {
            InitializeComponent();

            SaveToGeneralConfig = false;
            // intialize ListView callbacks
            //listViewDevices.ItemChecked += ListViewDevicesItemChecked;

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

            speedHeader.Text = Translations.Tr("H/s");
            secondarySpeedHeader.Text = Translations.Tr("H/s (Secondary)");
        }

        protected override void ListViewDevicesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_ignoreChecks) return;
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
            var allIndices = _indexTotals.SelectMany(i => i).ToList();
            var inactiveIndices = new List<int>();
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

                if (!allIndices.Contains(computeDevice.Index))
                    inactiveIndices.Add(computeDevice.Index);
            }

            foreach (var group in listViewDevices.Groups)
            {
                if (!(group is ListViewGroup g)) continue;

                if (g.Tag is List<int> indices)
                {
                    foreach (var lvi in listViewDevices.Items)
                    {
                        if (lvi is ListViewItem item && item.Tag is ComputeDevice dev &&
                            indices.Any(i => dev.Index == i))
                        {
                            g.Items.Add(item);
                        }
                    }

                    if (g.Items.Count <= 0) continue;

                    var t = SetTotalRow(indices, numItems);
                    g.Items.Add(t);
                }
                else if (g.Name == DefaultKey)
                {
                    foreach (var lvi in listViewDevices.Items)
                    {
                        if (lvi is ListViewItem item && item.Tag is ComputeDevice dev &&
                            inactiveIndices.Contains(dev.Index))
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

        private ListViewItem SetTotalRow(List<int> indices, int numSubs)
        {
            var total = new ListViewItem
            {
                Text = Translations.Tr("Total"),
                Tag = indices
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
                
                SetDiagText(item, 0, (int) dev.Load);
                SetDiagText(item, 1, (int) dev.Temp);
                SetDiagText(item, 2, dev.FanSpeed);
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

            lvi.SubItems[PowerUsage].Text = powerUsage.ToString("F2");
            lvi.SubItems[PowerCost].Text = powerCost.ToString("F2");
            lvi.SubItems[PowerProfit].Text = profit?.ToString("F2") ?? "";
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
            if (InvokeRequired)
            {
                Invoke((Action) ClearRatesAll);
            }
            else
            {
                _indexTotals.Clear();
                listViewDevices.Groups.Clear();
                UpdateListView();
            }
        }

        public void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException)
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

            // ID for algo/miner combo
            var key = string.Join(",", iApiData.DeviceIndices);
            // If index is not in any of the groups of indices
            if (!_indexTotals.Any(l => iApiData.DeviceIndices.All(l.Contains)))
            {
                _indexTotals.Add(new List<int>(iApiData.DeviceIndices));
            }
            // Make group for this algo/miner combo if not made already
            if (listViewDevices.Groups[key] == null)
            {
                var name = AlgorithmNiceHashNames.GetName(Helpers.DualAlgoFromAlgos(iApiData.AlgorithmID, iApiData.SecondaryAlgorithmID));
                var group = new ListViewGroup(key, name)
                {
                    Tag = iApiData.DeviceIndices
                };
                listViewDevices.Groups.Add(group);

                UpdateListView();
            }

            foreach (var lvi in listViewDevices.Items)
            {
                if (!(lvi is ListViewItem item)) continue;

                if (item.Tag is List<int> indices && indices.Same(iApiData.DeviceIndices))
                {
                    // This is a total row
                    UpdateRowInfo(item, iApiData.Speed, iApiData.SecondarySpeed, iApiData.Revenue, iApiData.Profit, iApiData.PowerCost, iApiData.PowerUsage);
                }
                else if (item.Tag is ComputeDevice dev && iApiData.DeviceIndices.Any(i => i == dev.Index))
                {
                    // This is a dev row
                    iApiData.PowerMap.TryGetValue(dev.Index, out var power);
                    var powerCostBtc = iApiData.PowerCostForIndex(dev.Index);

                    if (iApiData is SplitApiData split)
                    {
                        // Here we know per-device profits from API
                        split.Speeds.TryGetValue(dev.Index, out var speed);
                        split.SecondarySpeeds.TryGetValue(dev.Index, out var secSpeed);

                        UpdateRowInfo(item, speed, secSpeed, split.RevenueForIndex(dev.Index), split.ProfitForIndex(dev.Index), powerCostBtc, power);
                    }
                    else
                    {
                        // Here we only know total profit from miner
                        var powerCost = ExchangeRateApi.ConvertFromBtc(powerCostBtc * TimeFactor.TimeUnit);

                        SetPowerText(item, power, powerCost, null);
                    }
                }
            }

            GlobalRates?.UpdateGlobalRate();
        }

        public void ShowNotProfitable(string msg)
        {
        }

        public void HideNotProfitable()
        {
        }

        public void ForceMinerStatsUpdate()
        {
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
        
        private static void UpdateRowInfo(ListViewItem item, double speed, double secSpeed, double revenue, double profit,
            double power, double powerUsage)
        {
            try
            {
                item.SubItems[Speed].Text = Helpers.FormatSpeedOutput(speed);
                item.SubItems[SecSpeed].Text = secSpeed > 0 ? Helpers.FormatSpeedOutput(secSpeed) : "";

                var fiat = ExchangeRateApi.ConvertFromBtc(profit * TimeFactor.TimeUnit);
                if (ShowPowerCols)
                {
                    // When showing power cols, the default "profit" header changes to revenue
                    // The power headers then explain cost of power and real profit after subtracting this
                    item.SubItems[Profit].Text = (revenue * 1000 * TimeFactor.TimeUnit).ToString("F4");
                    item.SubItems[Fiat].Text = ExchangeRateApi.ConvertFromBtc(revenue * TimeFactor.TimeUnit).ToString("F2");
                    var powerCost = ExchangeRateApi.ConvertFromBtc(power * TimeFactor.TimeUnit);
                    SetPowerText(item, powerUsage, powerCost, fiat);
                }
                else
                {
                    item.SubItems[Profit].Text = (profit * 1000 * TimeFactor.TimeUnit).ToString("F4");
                    item.SubItems[Fiat].Text = fiat.ToString("F2");
                }
            }
            catch { }
        }

        #endregion
    }
}
