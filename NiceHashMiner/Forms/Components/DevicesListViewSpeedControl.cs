using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner.Forms.Components
{
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

        public DevicesListViewSpeedControl()
        {
            InitializeComponent();

            SaveToGeneralConfig = false;
            // intialize ListView callbacks
            listViewDevices.ItemChecked += ListViewDevicesItemChecked;

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
            listViewDevices.CheckBoxes = !isMining;
            Enabled = !isMining;
        }

        public override void InitLocale()
        {
            base.InitLocale();

            speedHeader.Text = International.GetText("Form_DevicesListViewSpeed_Hs");
            secondarySpeedHeader.Text = International.GetText("Form_DevicesListViewSpeed_SecondaryHs");
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
                SetOptionalHeaders(PowerKey);
                numItems += 3;
            }

            if (ShowDiagCols)
            { 
                SetOptionalHeaders(DiagKey);
                numItems += 3;
            }

            // set devices
            var lastIndex = 0;
            var allIndices = _indexTotals.SelectMany(i => i).ToList();
            var inactiveIndices = new List<int>();
            var endIndex = -1;
            foreach (var computeDevice in _devices)
            {
                //if (_indexTotals.Count > lastIndex && allIndices.Contains(computeDevice.Index))
                //{
                //    if (!_indexTotals[lastIndex].Contains(computeDevice.Index))
                //    {
                //        SetTotalRow(_indexTotals[lastIndex], endIndex, numItems);
                //        lastIndex++;
                //    }
                //}

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

                //lvi.SubItems.Add(computeDevice.Name);
                listViewDevices.Items.Add(lvi);

                //SetLvi(lvi, computeDevice.Index);
                endIndex = computeDevice.Index;

                if (!allIndices.Contains(computeDevice.Index))
                    inactiveIndices.Add(computeDevice.Index);
            }

            //if (endIndex > 0 && _indexTotals.Count > lastIndex && allIndices.Contains(endIndex))
            //{
            //    SetTotalRow(_indexTotals[lastIndex], endIndex, numItems);
            //}

            foreach (var group in listViewDevices.Groups)
            {
                if (!(@group is ListViewGroup g)) continue;

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

                    var t = SetTotalRow(indices, endIndex++, numItems);
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

        private ListViewItem SetTotalRow(List<int> indices, int index, int numSubs)
        {
            var total = new ListViewItem
            {
                Text = International.GetText("Form_DevicesListViewSpeed_Total"),
                Tag = indices
            };
            for (var i = 0; i < numSubs; i++)
            {
                total.SubItems.Add(new ListViewItem.ListViewSubItem());
            }
            listViewDevices.Items.Add(total);
            return total;
            //SetLvi(total, index);
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

        private static string CurrencyPerTimeUnit()
        {
            return FormatPerTimeUnit(ExchangeRateApi.ActiveDisplayCurrency);
        }

        private static string FormatPerTimeUnit(string unit)
        {
            var timeUnit = International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            return $"{unit}/{timeUnit}";
        }

        protected override void DevicesListViewEnableControl_Resize(object sender, EventArgs e)
        {
        }

        #endregion

        private static ListViewGroup CreateDefaultGroup()
        {
            return new ListViewGroup(DefaultKey, International.GetText("Form_DevicesListViewSpeed_Disabled"));
        }

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

        private void SetPowerText(ListViewItem lvi, double powerUsage, double powerCost, double? profit)
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
                        langKey = "Form_Settings_Algo_PowerUsage";
                        break;
                    case 1:
                        return International.GetText("Form_DevicesListViewSpeed_PowerCost", CurrencyPerTimeUnit());
                    case 2:
                        return International.GetText("Form_DevicesListViewSpeed_Profit", CurrencyPerTimeUnit());
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        langKey = "Form_DevicesListViewSpeed_Load";
                        break;
                    case 1:
                        langKey = "Form_DevicesListViewSpeed_Temp";
                        break;
                    case 2:
                        langKey = "Form_DevicesListViewSpeed_RPM";
                        break;
                }
            }

            return International.GetText(langKey);
        }

        #endregion

        #region IRatesCommunication Implementation

        public void ClearRatesAll()
        {
            _indexTotals.Clear();
            listViewDevices.Groups.Clear();
            UpdateListView();
        }

        private void UpdateRowInfo(ListViewItem item, double speed, double secSpeed, double revenue, double profit,
            double power, double powerUsage)
        {
            try
            {
                item.SubItems[Speed].Text = Helpers.FormatSpeedOutput(speed);
                if (secSpeed > 0)
                    item.SubItems[SecSpeed].Text = Helpers.FormatSpeedOutput(secSpeed);

                var fiat = ExchangeRateApi.ConvertFromBtc(profit * TimeFactor.TimeUnit);
                if (ShowPowerCols)
                {
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

        public void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException)
        {
            Enabled = true;

            // Ensure we have disabled group
            if (listViewDevices.Groups[DefaultKey] == null)
            {
                listViewDevices.Groups.Clear();
                listViewDevices.Groups.Add(CreateDefaultGroup());
            }

            var key = string.Join(",", iApiData.DeviceIndices);
            if (!_indexTotals.Any(l => iApiData.DeviceIndices.All(l.Contains)))
            {
                _indexTotals.Add(new List<int>(iApiData.DeviceIndices));
            }
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
                    UpdateRowInfo(item, iApiData.Speed, iApiData.SecondarySpeed, iApiData.Revenue, iApiData.Profit, iApiData.PowerCost, iApiData.PowerUsage);
                }
                else if (item.Tag is ComputeDevice dev && iApiData.DeviceIndices.Any(i => i == dev.Index))
                {
                    iApiData.PowerMap.TryGetValue(dev.Index, out var power);
                    var powerCostBtc = iApiData.PowerCostForIndex(dev.Index);

                    if (iApiData is SplitApiData split)
                    {
                        split.Speeds.TryGetValue(dev.Index, out var speed);
                        split.SecondarySpeeds.TryGetValue(dev.Index, out var secSpeed);

                        UpdateRowInfo(item, speed, secSpeed, split.RevenueForIndex(dev.Index), split.ProfitForIndex(dev.Index), powerCostBtc, power);
                    }
                    else
                    {
                        var powerCost = ExchangeRateApi.ConvertFromBtc(powerCostBtc * TimeFactor.TimeUnit);

                        SetPowerText(item, power, powerCost, null);
                    }
                }
            }

            GlobalRates?.UpdateGlobalRate();

            //var header = AlgorithmNiceHashNames.GetName(iApiData.AlgorithmID) + $"    {Helpers.FormatSpeedOutput(iApiData.Speed)}";
            //if (iApiData.SecondaryAlgorithmID != AlgorithmType.NONE)
            //{
            //    header += $"    {Helpers.FormatSpeedOutput(iApiData.SecondarySpeed)}";
            //}

            //header += $"    {FormatPayingOutput(paying)}    {ExchangeRateApi.GetCurrencyString(paying * FactorTimeUnit)}";

            //listViewDevices.BeginUpdate();
            //listViewDevices.Groups[key].Header = header;
            //listViewDevices.EndUpdate();
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

        public void ClearRates(int groupCount)
        {
        }

        #endregion
    }
}
