using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
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

        private const string PowerKey = "power";
        private const string DiagKey = "diag";

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

        public void SetIsMining(bool isMining)
        {
            listViewDevices.CheckBoxes = !isMining;
            Enabled = !isMining;
        }

        public override void InitLocale()
        {
            base.InitLocale();
            // TODO
        }

        #region ListView updating

        public override void SetComputeDevices(List<ComputeDevice> devices)
        {
            _devices = devices;
            UpdateListView();

            if (!ConfigManager.GeneralConfig.ShowDiagColumns) return;
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
            if (ConfigManager.GeneralConfig.ShowPowerColumns)
            {
                SetOptionalHeaders(PowerKey);
                numItems += 3;
            }

            if (ConfigManager.GeneralConfig.ShowDiagColumns)
            { 
                SetOptionalHeaders(DiagKey);
                numItems += 3;
            }

            // set devices
            var lastIndex = 0;
            var allIndices = _indexTotals.SelectMany(i => i).ToList();
            var endIndex = -1;
            foreach (var computeDevice in _devices)
            {
                if (_indexTotals.Count > lastIndex && allIndices.Contains(computeDevice.Index))
                {
                    if (!_indexTotals[lastIndex].Contains(computeDevice.Index))
                    {
                        SetTotalRow(_indexTotals[lastIndex], endIndex);
                        lastIndex++;
                    }
                }

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

                SetLvi(lvi, computeDevice.Index);
                endIndex = computeDevice.Index;
            }

            if (endIndex > 0 && _indexTotals.Count > lastIndex && allIndices.Contains(endIndex))
            {
                SetTotalRow(_indexTotals[lastIndex], endIndex);
            }

            listViewDevices.EndUpdate();
            listViewDevices.Invalidate(true);
            // reset properties
            SaveToGeneralConfig = true;
        }

        private void SetTotalRow(List<int> indices, int index)
        {
            var total = new ListViewItem
            {
                Text = "Total",
                Tag = indices
            };
            for (var i = 0; i < 4; i++)
            {
                total.SubItems.Add(new ListViewItem.ListViewSubItem());
            }
            listViewDevices.Items.Add(total);
            SetLvi(total, index);
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
            var timeUnit = International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            profitHeader.Text = $"mBTC/{timeUnit}";
            fiatHeader.Text = $"{ExchangeRateApi.ActiveDisplayCurrency}/{timeUnit}";
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
                Checked = ConfigManager.GeneralConfig.ShowPowerColumns
            };
            var showDiag = new ToolStripMenuItem("Show Diagnostic Info")
            {
                Tag = DiagKey,
                Checked = ConfigManager.GeneralConfig.ShowDiagColumns
            };

            showPower.Click += SetPowerHeaders;
            showDiag.Click += SetDiagHeaders;

            contextMenuStrip1.Items.Add(showPower);
            contextMenuStrip1.Items.Add(showDiag);
            contextMenuStrip1.Show(Cursor.Position);
        }

        private void SetPowerHeaders(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.ShowPowerColumns = !ConfigManager.GeneralConfig.ShowPowerColumns;
            UpdateListView();
        }

        private void SetDiagHeaders(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.ShowDiagColumns = !ConfigManager.GeneralConfig.ShowDiagColumns;
            UpdateListView();

            if (ConfigManager.GeneralConfig.ShowDiagColumns)
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

            var start = ConfigManager.GeneralConfig.ShowPowerColumns ? 8 : 5;
            if (item.SubItems.Count <= start + index) return;
            item.SubItems[start + index].Text = value.ToString();
        }

        private void SetOptionalHeaders(string key)
        {
            for (var i = 0; i < 3; i++)
            {
                if (listViewDevices.Columns.ContainsKey($"{key}{i}"))
                    continue;

                listViewDevices.Columns.Add($"{key}{i}", $"{key}{i}", 60, HorizontalAlignment.Right, "");
            }
        }

        private void RemoveOptionalHeaders(string key)
        {
            for (var i = 0; i < 3; i++)
            {
                listViewDevices.Columns.RemoveByKey($"{key}{i}");
            }
        }

        #endregion

        #region IRatesCommunication Implementation

        public void ClearRatesAll()
        {
            _indexTotals.Clear();
            listViewDevices.Groups.Clear();
            UpdateListView();
        }

        public void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException)
        {
            Enabled = true;

            var key = string.Join(",", iApiData.DeviceIndices);
            if (!_indexTotals.Any(l => iApiData.DeviceIndices.All(l.Contains)))
            {
                _indexTotals.Add(new List<int>(iApiData.DeviceIndices));
            }
            if (listViewDevices.Groups[key] == null)
            {
                var group = new ListViewGroup(key, AlgorithmNiceHashNames.GetName(iApiData.AlgorithmID))
                {
                    Tag = iApiData.DeviceIndices
                };
                listViewDevices.Groups.Add(group);

                UpdateListView();
            }

            foreach (var lvi in listViewDevices.Items)
            {
                if (lvi is ListViewItem item && item.Tag is List<int> indices &&
                    indices.Same(iApiData.DeviceIndices))
                {
                    try
                    {
                        item.SubItems[Speed].Text = Helpers.FormatSpeedOutput(iApiData.Speed);
                        if (iApiData.SecondaryAlgorithmID != AlgorithmType.NONE)
                            item.SubItems[SecSpeed].Text = Helpers.FormatSpeedOutput(iApiData.SecondarySpeed);
                        item.SubItems[Profit].Text = (paying * 1000 * TimeFactor.TimeUnit).ToString("F4");
                        item.SubItems[Fiat].Text = ExchangeRateApi.ConvertFromBtc(paying * TimeFactor.TimeUnit).ToString("F2");
                    }
                    catch { }
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
