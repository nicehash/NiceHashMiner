using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Drawing;
using System.Windows.Forms;
using NiceHashMiner.Algorithms;

namespace NiceHashMiner.Forms.Components
{
    public partial class AlgorithmsListView : UserControl
    {
        public enum Column : int
        {
            ENABLED = 0,
            ALGORITHM,
            MINER,
            SPEEDS,
            RATIO,
            RATE,
        }

        public interface IAlgorithmsListView
        {
            void SetCurrentlySelected(ListViewItem lvi, ComputeDevice computeDevice);
            void HandleCheck(ListViewItem lvi);
            void ChangeSpeed(ListViewItem lvi);
        }

        public IAlgorithmsListView ComunicationInterface { get; set; }

        public IBenchmarkCalculation BenchmarkCalculation { get; set; }

        ComputeDevice _computeDevice;

        private class DefaultAlgorithmColorSeter : IListItemCheckColorSetter
        {
            private static readonly Color DisabledColor = Color.DarkGray;
            private static readonly Color BenchmarkedColor = Color.LightGreen;
            private static readonly Color UnbenchmarkedColor = Color.LightBlue;

            public void LviSetColor(ListViewItem lvi)
            {
                if (lvi.Tag is Algorithm algorithm)
                {
                    if (!algorithm.Enabled && !algorithm.IsBenchmarkPending)
                    {
                        lvi.BackColor = DisabledColor;
                    }
                    else if (!algorithm.BenchmarkNeeded && !algorithm.IsBenchmarkPending)
                    {
                        lvi.BackColor = BenchmarkedColor;
                    }
                    else
                    {
                        lvi.BackColor = UnbenchmarkedColor;
                    }
                }
            }
        }

        private readonly IListItemCheckColorSetter _listItemCheckColorSetter = new DefaultAlgorithmColorSeter();

        // disable checkboxes when in benchmark mode
        private bool _isInBenchmark = false;

        // helper for benchmarking logic
        public bool IsInBenchmark
        {
            get => _isInBenchmark;
            set
            {
                if (value)
                {
                    _isInBenchmark = true;
                    listViewAlgorithms.CheckBoxes = false;
                }
                else
                {
                    _isInBenchmark = false;
                    listViewAlgorithms.CheckBoxes = true;
                }
            }
        }

        public AlgorithmsListView()
        {
            InitializeComponent();
            // callback initializations
            listViewAlgorithms.ItemSelectionChanged += ListViewAlgorithms_ItemSelectionChanged;
            listViewAlgorithms.ItemChecked += (ItemCheckedEventHandler) ListViewAlgorithms_ItemChecked;
            IsInBenchmark = false;
        }

        public void SetAlgorithms(ComputeDevice computeDevice, bool isEnabled)
        {
            _computeDevice = computeDevice;
            listViewAlgorithms.BeginUpdate();
            listViewAlgorithms.Items.Clear();
            foreach (var alg in computeDevice.AlgorithmSettings)
            {
                var lvi = new ListViewItem();

                var name = alg.AlgorithmName;
                var minerName = alg.MinerBaseTypeName;
                var payingRatio = alg.CurPayingRatio;

                lvi.SubItems.Add(name);

                //sub.Tag = alg.Value;
                lvi.SubItems.Add(minerName);
                lvi.SubItems.Add(alg.BenchmarkSpeedString());
                lvi.SubItems.Add(payingRatio);
                lvi.SubItems.Add(alg.CurPayingRateStr);
                lvi.Tag = alg;
                lvi.Checked = alg.Enabled;
                listViewAlgorithms.Items.Add(lvi);
            }

            listViewAlgorithms.EndUpdate();
            Enabled = isEnabled;
        }

        public void RepaintStatus(bool isEnabled, string uuid)
        {
            if (_computeDevice != null && _computeDevice.Uuid == uuid)
            {
                foreach (ListViewItem lvi in listViewAlgorithms.Items)
                {
                    var algo = lvi.Tag as Algorithm;
                    lvi.SubItems[(int)Column.SPEEDS].Text = algo?.BenchmarkSpeedString();
                    _listItemCheckColorSetter.LviSetColor(lvi);
                }

                Enabled = isEnabled;
            }
        }

        #region Callbacks Events

        private void ListViewAlgorithms_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ComunicationInterface?.SetCurrentlySelected(e.Item, _computeDevice);
        }

        private void ListViewAlgorithms_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (IsInBenchmark)
            {
                e.Item.Checked = !e.Item.Checked;
                return;
            }

            if (e.Item.Tag is Algorithm algo)
            {
                algo.Enabled = e.Item.Checked;
            }

            ComunicationInterface?.HandleCheck(e.Item);
            var lvi = e.Item;
            _listItemCheckColorSetter.LviSetColor(lvi);
            // update benchmark status data
            BenchmarkCalculation?.CalcBenchmarkDevicesAlgorithmQueue();
        }

        #endregion //Callbacks Events

        public void ResetListItemColors()
        {
            foreach (ListViewItem lvi in listViewAlgorithms.Items)
            {
                _listItemCheckColorSetter?.LviSetColor(lvi);
            }
        }

        // benchmark settings
        public void SetSpeedStatus(ComputeDevice computeDevice, Algorithm algorithm, string status)
        {
            if (algorithm != null)
            {
                algorithm.BenchmarkStatus = status;
                // gui update only if same as selected
                if (_computeDevice != null && computeDevice.Uuid == _computeDevice.Uuid)
                {
                    foreach (ListViewItem lvi in listViewAlgorithms.Items)
                    {
                        if (lvi.Tag is Algorithm algo && algo.AlgorithmStringID == algorithm.AlgorithmStringID)
                        {
                            // TODO handle numbers
                            lvi.SubItems[(int)Column.SPEEDS].Text = algorithm.BenchmarkSpeedString();
                            lvi.SubItems[(int)Column.RATE].Text = algorithm.CurPayingRateStr;
                            // TODO handle DUAL first + second paying ratio X+Y
                            lvi.SubItems[(int)Column.RATIO].Text = algorithm.CurPayingRatio;

                            _listItemCheckColorSetter.LviSetColor(lvi);
                            break;
                        }
                    }
                }
            }
        }

        private void ListViewAlgorithms_MouseClick(object sender, MouseEventArgs e)
        {
            if (IsInBenchmark) return;
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Items.Clear();
                // disable all
                {
                    var disableAllItems = new ToolStripMenuItem
                    {
                        Text = Translations.Tr("Disable All Algorithms")
                    };
                    disableAllItems.Click += ToolStripMenuItemDisableAll_Click;
                    contextMenuStrip1.Items.Add(disableAllItems);
                }
                // enable all
                {
                    var enableAllItems = new ToolStripMenuItem
                    {
                        Text = Translations.Tr("Enable All Algorithms")
                    };
                    enableAllItems.Click += ToolStripMenuItemEnableAll_Click;
                    contextMenuStrip1.Items.Add(enableAllItems);
                }
                // test this
                {
                    var testItem = new ToolStripMenuItem
                    {
                        Text = Translations.Tr("Enable Only This")
                    };
                    testItem.Click += ToolStripMenuItemTest_Click;
                    contextMenuStrip1.Items.Add(testItem);
                }
                // enable benchmarked only
                {
                    var enableBenchedItem = new ToolStripMenuItem
                    {
                        Text = Translations.Tr("Enable Benchmarked Only")
                    };
                    enableBenchedItem.Click += ToolStripMenuItemEnableBenched_Click;
                    contextMenuStrip1.Items.Add(enableBenchedItem);
                }
                // clear item
                {
                    var clearItem = new ToolStripMenuItem
                    {
                        Text = Translations.Tr("Clear Algorithm Speed")
                    };
                    clearItem.Click += ToolStripMenuItemClear_Click;
                    contextMenuStrip1.Items.Add(clearItem);
                }
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void ToolStripMenuItemEnableAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listViewAlgorithms.Items)
            {
                lvi.Checked = true;
            }
        }

        private void ToolStripMenuItemDisableAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listViewAlgorithms.Items)
            {
                lvi.Checked = false;
            }
        }

        private void ToolStripMenuItemClear_Click(object sender, EventArgs e)
        {
            if (_computeDevice != null)
            {
                foreach (ListViewItem lvi in listViewAlgorithms.SelectedItems)
                {
                    if (lvi.Tag is Algorithm algorithm)
                    {
                        algorithm.ClearSpeeds();
                        RepaintStatus(_computeDevice.Enabled, _computeDevice.Uuid);
                        // update benchmark status data
                        BenchmarkCalculation?.CalcBenchmarkDevicesAlgorithmQueue();
                        // update settings
                        ComunicationInterface?.ChangeSpeed(lvi);
                    }
                }
            }
        }

        private void ToolStripMenuItemTest_Click(object sender, EventArgs e)
        {
            if (_computeDevice != null)
            {
                foreach (ListViewItem lvi in listViewAlgorithms.Items)
                {
                    if (lvi.Tag is Algorithm algorithm)
                    {
                        lvi.Checked = lvi.Selected;
                        if (lvi.Selected && algorithm.BenchmarkSpeed <= 0)
                        {
                            // If it has zero speed, set to 1 so it can be tested, must be available only in DEBUG mode!!
                            #if DEBUG
                            algorithm.BenchmarkSpeed = 1;
                            #endif
                            RepaintStatus(_computeDevice.Enabled, _computeDevice.Uuid);
                            ComunicationInterface?.ChangeSpeed(lvi);
                        }
                    }
                }
            }
        }

        private void ToolStripMenuItemEnableBenched_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listViewAlgorithms.Items)
            {
                if (lvi.Tag is Algorithm algorithm && algorithm.BenchmarkSpeed > 0)
                {
                    lvi.Checked = true;
                }
            }
        }
    }
}
