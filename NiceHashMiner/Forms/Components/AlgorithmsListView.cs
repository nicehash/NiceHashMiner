using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NiceHashMiner.Algorithms;

namespace NiceHashMiner.Forms.Components
{
    public partial class AlgorithmsListView : UserControl
    {
        private const int ENABLED = 0;
        private const int ALGORITHM = 1;
        private const int SPEED = 2;
        private const int SECSPEED = 3;
        private const int RATIO = 4;
        private const int RATE = 5;

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

        public void InitLocale()
        {
            listViewAlgorithms.Columns[ENABLED].Text = International.GetText("AlgorithmsListView_Enabled");
            listViewAlgorithms.Columns[ALGORITHM].Text = International.GetText("AlgorithmsListView_Algorithm");
            listViewAlgorithms.Columns[SPEED].Text = International.GetText("AlgorithmsListView_Speed");
            listViewAlgorithms.Columns[SECSPEED].Text = International.GetText("Form_DcriValues_SecondarySpeed");
            listViewAlgorithms.Columns[RATIO].Text = International.GetText("AlgorithmsListView_Ratio");
            listViewAlgorithms.Columns[RATE].Text = International.GetText("AlgorithmsListView_Rate");
        }

        public void SetAlgorithms(ComputeDevice computeDevice, bool isEnabled)
        {
            _computeDevice = computeDevice;
            listViewAlgorithms.BeginUpdate();
            listViewAlgorithms.Items.Clear();
            foreach (var alg in computeDevice.GetAlgorithmSettings())
            {
                var lvi = new ListViewItem();

                var name = "";
                var secondarySpeed = "";
                var payingRatio = "";
                if (alg is DualAlgorithm dualAlg)
                {
                    name = "  + " + dualAlg.SecondaryAlgorithmName;
                    secondarySpeed = dualAlg.SecondaryBenchmarkSpeedString();
                    payingRatio = dualAlg.SecondaryCurPayingRatio;
                }
                else
                {
                    name = $"{alg.AlgorithmName} ({alg.MinerBaseTypeName})";
                    payingRatio = alg.CurPayingRatio;
                }

                lvi.SubItems.Add(name);

                //sub.Tag = alg.Value;
                lvi.SubItems.Add(alg.BenchmarkSpeedString());
                lvi.SubItems.Add(secondarySpeed);
                lvi.SubItems.Add(payingRatio);
                lvi.SubItems.Add(alg.CurPayingRate);
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
                    lvi.SubItems[SPEED].Text = algo?.BenchmarkSpeedString();
                    if (algo is DualAlgorithm dualAlg)
                        lvi.SubItems[SECSPEED].Text = dualAlg.SecondaryBenchmarkSpeedString();
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
                            lvi.SubItems[SPEED].Text = algorithm.BenchmarkSpeedString();
                            lvi.SubItems[RATE].Text = algorithm.CurPayingRate;
                            
                            if (algorithm is DualAlgorithm dualAlg)
                            {
                                lvi.SubItems[RATIO].Text = dualAlg.SecondaryCurPayingRatio;
                                lvi.SubItems[SECSPEED].Text = dualAlg.SecondaryBenchmarkSpeedString();
                            }
                            else
                            {
                                lvi.SubItems[RATIO].Text = algorithm.CurPayingRatio;
                            }

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
                        Text = International.GetText("AlgorithmsListView_ContextMenu_DisableAll")
                    };
                    disableAllItems.Click += ToolStripMenuItemDisableAll_Click;
                    contextMenuStrip1.Items.Add(disableAllItems);
                }
                // enable all
                {
                    var enableAllItems = new ToolStripMenuItem
                    {
                        Text = International.GetText("AlgorithmsListView_ContextMenu_EnableAll")
                    };
                    enableAllItems.Click += ToolStripMenuItemEnableAll_Click;
                    contextMenuStrip1.Items.Add(enableAllItems);
                }
                // test this
                {
                    var testItem = new ToolStripMenuItem
                    {
                        Text = International.GetText("AlgorithmsListView_ContextMenu_TestItem")
                    };
                    testItem.Click += ToolStripMenuItemTest_Click;
                    contextMenuStrip1.Items.Add(testItem);
                }
                // enable benchmarked only
                {
                    var enableBenchedItem = new ToolStripMenuItem
                    {
                        Text = International.GetText("AlgorithmsListView_ContextMenu_EnableBenched")
                    };
                    enableBenchedItem.Click += ToolStripMenuItemEnableBenched_Click;
                    contextMenuStrip1.Items.Add(enableBenchedItem);
                }
                // clear item
                {
                    var clearItem = new ToolStripMenuItem
                    {
                        Text = International.GetText("AlgorithmsListView_ContextMenu_ClearItem")
                    };
                    clearItem.Click += ToolStripMenuItemClear_Click;
                    contextMenuStrip1.Items.Add(clearItem);
                }
                // open dcri
                {
                    var dcriMenu = new ToolStripMenuItem
                    {
                        Text = International.GetText("Form_DcriValues_Title")
                    };

                    if (listViewAlgorithms.SelectedItems.Count > 0
                        && listViewAlgorithms.SelectedItems[0].Tag is DualAlgorithm dualAlg)
                    {
                        dcriMenu.Enabled = true;

                        var openDcri = new ToolStripMenuItem
                        {
                            Text = International.GetText("AlgorithmsListView_ContextMenu_OpenDcri")
                        };
                        openDcri.Click += toolStripMenuItemOpenDcri_Click;
                        dcriMenu.DropDownItems.Add(openDcri);

                        var tuningEnabled = new ToolStripMenuItem
                        {
                            Text = International.GetText("Form_DcriValues_TuningEnabled"),
                            CheckOnClick = true,
                            Checked = dualAlg.TuningEnabled
                        };
                        tuningEnabled.CheckedChanged += toolStripMenuItemTuningEnabled_Checked;
                        dcriMenu.DropDownItems.Add(tuningEnabled);
                    }
                    else
                    {
                        dcriMenu.Enabled = false;
                    }

                    contextMenuStrip1.Items.Add(dcriMenu);
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
                        algorithm.BenchmarkSpeed = 0;
                        if (algorithm is DualAlgorithm dualAlgo)
                        {
                            dualAlgo.SecondaryBenchmarkSpeed = 0;
                            dualAlgo.IntensitySpeeds = new Dictionary<int, double>();
                            dualAlgo.SecondaryIntensitySpeeds = new Dictionary<int, double>();
                            dualAlgo.IntensityUpToDate = false;
                        }

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
                            // If it has zero speed, set to 1 so it can be tested
                            algorithm.BenchmarkSpeed = 1;
                            RepaintStatus(_computeDevice.Enabled, _computeDevice.Uuid);
                            ComunicationInterface?.ChangeSpeed(lvi);
                        }
                    }
                }
            }
        }

        private void toolStripMenuItemOpenDcri_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listViewAlgorithms.SelectedItems)
            {
                if (lvi.Tag is DualAlgorithm algo)
                {
                    var dcriValues = new FormDcriValues(algo);
                    dcriValues.ShowDialog();
                    RepaintStatus(_computeDevice.Enabled, _computeDevice.Uuid);
                    // update benchmark status data
                    BenchmarkCalculation?.CalcBenchmarkDevicesAlgorithmQueue();
                    // update settings
                    ComunicationInterface?.ChangeSpeed(lvi);
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

        private void toolStripMenuItemTuningEnabled_Checked(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listViewAlgorithms.SelectedItems)
            {
                if (lvi.Tag is DualAlgorithm algo)
                {
                    algo.TuningEnabled = ((ToolStripMenuItem) sender).Checked;
                    RepaintStatus(_computeDevice.Enabled, _computeDevice.Uuid);
                }
            }
        }
    }
}
