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
    public partial class DevicesListViewSpeedControl : DevicesListViewEnableControl, IRatesComunication
    {
        private const int ENABLED = 0;
        private const int Speed = 1;
        private const int SecSpeed = 2;
        private const int Profit = 3;
        private const int Fiat = 4;

        private class DefaultDevicesColorSeter : IListItemCheckColorSetter
        {
            private static readonly Color EnabledColor = Color.White;
            private static readonly Color DisabledColor = Color.DarkGray;

            public void LviSetColor(ListViewItem lvi)
            {
                if (lvi.Tag is ComputeDevice cdvo)
                {
                    lvi.BackColor = cdvo.Enabled ? EnabledColor : DisabledColor;
                }
            }
        }

        private IListItemCheckColorSetter _listItemCheckColorSetter = new DefaultDevicesColorSeter();

        public IBenchmarkCalculation BenchmarkCalculation { get; set; }

        private AlgorithmsListView _algorithmsListView;

        // disable checkboxes when in benchmark mode
        private bool _isInBenchmark;

        // helper for benchmarking logic
        public bool IsInBenchmark
        {
            get => _isInBenchmark;
            set
            {
                if (value)
                {
                    _isInBenchmark = true;
                    listViewDevices.CheckBoxes = false;
                }
                else
                {
                    _isInBenchmark = false;
                    listViewDevices.CheckBoxes = true;
                }
            }
        }

        private bool _isMining;

        public bool IsMining
        {
            get => _isMining;
            set
            {
                if (value)
                {
                    _isMining = true;
                    listViewDevices.CheckBoxes = false;
                }
                else
                {
                    _isMining = false;
                    listViewDevices.CheckBoxes = true;
                }
            }
        }

        private List<ComputeDevice> _devices;

        public double FactorTimeUnit;

        private List<List<int>> _indexTotals = new List<List<int>>();

        public DevicesListViewSpeedControl()
        {
            InitializeComponent();

            SaveToGeneralConfig = false;
            // intialize ListView callbacks
            listViewDevices.ItemChecked += ListViewDevicesItemChecked;
            //listViewDevices.CheckBoxes = false;
            IsMining = false;
            BenchmarkCalculation = null;
        }

        public void InitLocale()
        {
            listViewDevices.Columns[ENABLED].Text =
                International.GetText("ListView_Device"); //International.GetText("ListView_Enabled");
            //listViewDevices.Columns[DEVICE].Text = International.GetText("ListView_Device");
        }

        protected override void DevicesListViewEnableControl_Resize(object sender, EventArgs e)
        {
            // only one 
            //foreach (ColumnHeader ch in listViewDevices.Columns)
            //{
            //    ch.Width = Width - 10;
            //}
        }

        public override void SetComputeDevices(List<ComputeDevice> devices)
        {
            _devices = devices;
            base.SetComputeDevices(devices);
        }

        private void UpdateListView()
        {
            // to not run callbacks when setting new
            var tmpSaveToGeneralConfig = SaveToGeneralConfig;
            SaveToGeneralConfig = false;
            listViewDevices.BeginUpdate();
            listViewDevices.Items.Clear();
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
            SaveToGeneralConfig = tmpSaveToGeneralConfig;
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

        private string FormatPayingOutput(double paying)
        {
            string ret;

            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && paying < 0.1)
                ret = (paying * 1000 * FactorTimeUnit).ToString("F5", CultureInfo.InvariantCulture) + " mBTC/" +
                      International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            else
                ret = (paying * FactorTimeUnit).ToString("F6", CultureInfo.InvariantCulture) + " BTC/" +
                      International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());

            return ret;
        }

        #region IRatesCommunication Implementation

        public void ClearRatesAll()
        {
            _indexTotals.Clear();
            listViewDevices.Groups.Clear();
            UpdateListView();
        }

        public void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException)
        {
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
                        item.SubItems[Profit].Text = (paying * 1000).ToString("F4");
                        item.SubItems[Fiat].Text = ExchangeRateApi.ConvertFromBtc(paying).ToString("F2");
                    }
                    catch { }
                }
            }

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
