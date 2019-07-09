using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    /// <summary>
    /// List view to show algorithm info with disable/enable and bench status colours
    /// </summary>
    public partial class DevicesListViewBenchmarkControl : DevicesListViewEnableControl
    {
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
        
        public bool IsSettingsCopyEnabled = false;

        public DevicesListViewBenchmarkControl()
        {
            InitializeComponent();

            SaveToGeneralConfig = false;
            // intialize ListView callbacks
            listViewDevices.ItemChecked += ListViewDevicesItemChecked;
            IsMining = false;
            BenchmarkCalculation = null;
        }

        public void SetIListItemCheckColorSetter(IListItemCheckColorSetter listItemCheckColorSetter)
        {
            _listItemCheckColorSetter = listItemCheckColorSetter;
        }

        public void ResetListItemColors()
        {
            foreach (ListViewItem lvi in listViewDevices.Items)
            {
                _listItemCheckColorSetter?.LviSetColor(lvi);
            }
        }

        protected override void SetLvi(ListViewItem lvi, int index)
        {
            _listItemCheckColorSetter.LviSetColor(lvi);
        }

        protected override void ListViewDevicesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            base.ListViewDevicesItemChecked(sender, e);
            BenchmarkCalculation?.CalcBenchmarkDevicesAlgorithmQueue();
        }

        protected override void ListViewDevices_MouseClick(object sender, MouseEventArgs e)
        {
            if (IsInBenchmark) return;
            if (IsMining) return;
            if (e.Button == MouseButtons.Right)
            {
                if (listViewDevices.FocusedItem.Bounds.Contains(e.Location))
                {
                    contextMenuStrip1.Items.Clear();
                    if (IsSettingsCopyEnabled)
                    {
                        if (listViewDevices.FocusedItem.Tag is ComputeDevice cDevice)
                        {
                            var sameDevTypes =
                                AvailableDevices.GetSameDevicesTypeAsDeviceWithUuid(cDevice.Uuid);
                            if (sameDevTypes.Count > 0)
                            {
                                var copyBenchItem = new ToolStripMenuItem();
                                var copyTuningItem = new ToolStripMenuItem();
                                //copyBenchItem.DropDownItems
                                foreach (var cDev in sameDevTypes)
                                {
                                    if (cDev.Enabled)
                                    {
                                        var copyBenchDropDownItem = new ToolStripMenuItem
                                        {
                                            Text = cDev.Name,
                                            Checked = cDev.Uuid == cDevice.BenchmarkCopyUuid
                                        };
                                        copyBenchDropDownItem.Click += ToolStripMenuItemCopySettings_Click;
                                        copyBenchDropDownItem.Tag = cDev.Uuid;
                                        copyBenchItem.DropDownItems.Add(copyBenchDropDownItem);
                                        
                                        var copyTuningDropDownItem = new ToolStripMenuItem {
                                            Text = cDev.Name
                                            //Checked = cDev.UUID == CDevice.TuningCopyUUID
                                        };
                                        copyTuningDropDownItem.Click += ToolStripMenuItemCopyTuning_Click;
                                        copyTuningDropDownItem.Tag = cDev.Uuid;
                                        copyTuningItem.DropDownItems.Add(copyTuningDropDownItem);
                                    }
                                }
                                copyBenchItem.Text = Translations.Tr("Copy Settings From (Benchmarks, algorithm parameters, ...)");
                                copyTuningItem.Text = Translations.Tr("Copy tuning settings only");
                                contextMenuStrip1.Items.Add(copyBenchItem);
                                contextMenuStrip1.Items.Add(copyTuningItem);
                            }
                        }
                    }
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void ToolStripMenuItem_Click(object sender, bool justTuning) {
            if (sender is ToolStripMenuItem item && item.Tag is string uuid
                && listViewDevices.FocusedItem.Tag is ComputeDevice CDevice) {
                var copyBenchCDev = AvailableDevices.GetDeviceWithUuid(uuid);

                var result = MessageBox.Show(
                    string.Format(
                        Translations.Tr("Are you sure you want to copy settings from {0} to {1}?"),
                        copyBenchCDev.GetFullName(), CDevice.GetFullName()),
                    Translations.Tr("Confirm Settings Copy"),
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) 
                {
                    CDevice.BenchmarkCopyUuid = uuid;
                    CDevice.CopyBenchmarkSettingsFrom(copyBenchCDev);
                    _algorithmsListView?.RepaintStatus(CDevice.Enabled, CDevice.Uuid);
                }
            }
        }

        private void ToolStripMenuItemCopySettings_Click(object sender, EventArgs e) 
        {
            ToolStripMenuItem_Click(sender, false);
        }

        private void ToolStripMenuItemCopyTuning_Click(object sender, EventArgs e) 
        {
            ToolStripMenuItem_Click(sender, true);
        }
    }
}
