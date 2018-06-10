using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public partial class DevicesListViewSpeedControl : DevicesListViewEnableControl
    {
        private const int ENABLED = 0;
        private const int DEVICE = 1;

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
    }
}
