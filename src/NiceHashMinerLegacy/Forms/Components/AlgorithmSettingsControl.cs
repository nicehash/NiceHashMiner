using NiceHashMiner.Devices;
using System;
using System.Windows.Forms;
using NiceHashMiner.Algorithms;

namespace NiceHashMiner.Forms.Components
{
    public partial class AlgorithmSettingsControl : UserControl, AlgorithmsListView.IAlgorithmsListView
    {
        private ComputeDevice _computeDevice;
        private Algorithm _currentlySelectedAlgorithm;
        private ListViewItem _currentlySelectedLvi;

        // winform crappy event workarond
        private bool _selected = false;

        public AlgorithmSettingsControl()
        {
            InitializeComponent();
            fieldBoxBenchmarkSpeed.SetInputModeDoubleOnly();
            secondaryFieldBoxBenchmarkSpeed.SetInputModeDoubleOnly();
            field_PowerUsage.SetInputModeDoubleOnly();

            field_PowerUsage.SetOnTextLeave(PowerUsage_Leave);
            fieldBoxBenchmarkSpeed.SetOnTextChanged(TextChangedBenchmarkSpeed);
            secondaryFieldBoxBenchmarkSpeed.SetOnTextChanged(SecondaryTextChangedBenchmarkSpeed);
            richTextBoxExtraLaunchParameters.TextChanged += TextChangedExtraLaunchParameters;
        }

        public void Deselect()
        {
            _selected = false;
            groupBoxSelectedAlgorithmSettings.Text = string.Format(International.GetText("AlgorithmsListView_GroupBox"),
                International.GetText("AlgorithmsListView_GroupBox_NONE"));
            Enabled = false;
            fieldBoxBenchmarkSpeed.EntryText = "";
            secondaryFieldBoxBenchmarkSpeed.EntryText = "";
            field_PowerUsage.EntryText = "";
            richTextBoxExtraLaunchParameters.Text = "";
        }

        public void InitLocale(ToolTip toolTip1)
        {
            field_PowerUsage.InitLocale(toolTip1,
                International.GetText("Form_Settings_Algo_PowerUsage") + ":",
                International.GetText("Form_Settings_ToolTip_PowerUsage"));
            fieldBoxBenchmarkSpeed.InitLocale(toolTip1,
                International.GetText("Form_Settings_Algo_BenchmarkSpeed") + ":",
                International.GetText("Form_Settings_ToolTip_AlgoBenchmarkSpeed"));
            secondaryFieldBoxBenchmarkSpeed.InitLocale(toolTip1,
                International.GetText("Form_Settings_Algo_SecondaryBenchmarkSpeed") + ":",
                International.GetText("Form_Settings_ToolTip_AlgoSecondaryBenchmarkSpeed"));
            groupBoxExtraLaunchParameters.Text = International.GetText("Form_Settings_General_ExtraLaunchParameters");
            toolTip1.SetToolTip(groupBoxExtraLaunchParameters,
                International.GetText("Form_Settings_ToolTip_AlgoExtraLaunchParameters"));
            toolTip1.SetToolTip(pictureBox1, International.GetText("Form_Settings_ToolTip_AlgoExtraLaunchParameters"));
        }

        private static string ParseStringDefault(string value)
        {
            return value ?? "";
        }

        private static string ParseDoubleDefault(double value)
        {
            return value <= 0 ? "" : value.ToString();
        }

        public void SetCurrentlySelected(ListViewItem lvi, ComputeDevice computeDevice)
        {
            // should not happen ever
            if (lvi == null) return;

            _computeDevice = computeDevice;
            if (lvi.Tag is Algorithm algorithm)
            {
                _selected = true;
                _currentlySelectedAlgorithm = algorithm;
                _currentlySelectedLvi = lvi;
                Enabled = lvi.Checked;

                groupBoxSelectedAlgorithmSettings.Text = string.Format(
                    International.GetText("AlgorithmsListView_GroupBox"),
                    $"{algorithm.AlgorithmName} ({algorithm.MinerBaseTypeName})");
                ;

                field_PowerUsage.EntryText = ParseDoubleDefault(algorithm.PowerUsage);
                fieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(algorithm.BenchmarkSpeed);
                richTextBoxExtraLaunchParameters.Text = ParseStringDefault(algorithm.ExtraLaunchParameters);
                if (algorithm is DualAlgorithm dualAlgo) 
                {
                    secondaryFieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(dualAlgo.SecondaryBenchmarkSpeed);
                    secondaryFieldBoxBenchmarkSpeed.Enabled = true;
                } 
                else 
                {
                    secondaryFieldBoxBenchmarkSpeed.Enabled = false;
                }
                
                Update();
            } 
            else {
                // TODO this should not be null
            }
        }

        public void HandleCheck(ListViewItem lvi)
        {
            if (ReferenceEquals(_currentlySelectedLvi, lvi))
            {
                Enabled = lvi.Checked;
            }
        }

        public void ChangeSpeed(ListViewItem lvi)
        {
            if (ReferenceEquals(_currentlySelectedLvi, lvi))
            {
                if (lvi.Tag is Algorithm algorithm)
                {
                    fieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(algorithm.BenchmarkSpeed);
                    field_PowerUsage.EntryText = ParseDoubleDefault(algorithm.PowerUsage);
                    if (algorithm is DualAlgorithm dualAlgo) 
                    {
                        secondaryFieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(dualAlgo.SecondaryBenchmarkSpeed);
                    } 
                    else 
                    {
                        secondaryFieldBoxBenchmarkSpeed.EntryText = "0";
                    }
                }
            }
        }

        private bool CanEdit()
        {
            return _currentlySelectedAlgorithm != null && _selected;
        }

        #region Callbacks Events

        private void TextChangedBenchmarkSpeed(object sender, EventArgs e)
        {
            if (!CanEdit()) return;
            if (double.TryParse(fieldBoxBenchmarkSpeed.EntryText, out var value))
            {
                _currentlySelectedAlgorithm.BenchmarkSpeed = value;
            }
            UpdateSpeedText();
        }

        private void SecondaryTextChangedBenchmarkSpeed(object sender, EventArgs e)
        {
            if (double.TryParse(secondaryFieldBoxBenchmarkSpeed.EntryText, out var secondaryValue)
                && _currentlySelectedAlgorithm is DualAlgorithm dualAlgo)
            {
                dualAlgo.SecondaryBenchmarkSpeed = secondaryValue;
            }
            UpdateSpeedText();
        }

        private void UpdateSpeedText()
        {
            var secondarySpeed = (_currentlySelectedAlgorithm is DualAlgorithm dualAlgo) ? dualAlgo.SecondaryBenchmarkSpeed : 0;
            var speedString = Helpers.FormatDualSpeedOutput(_currentlySelectedAlgorithm.BenchmarkSpeed, secondarySpeed, _currentlySelectedAlgorithm.NiceHashID);
            // update lvi speed
            if (_currentlySelectedLvi != null)
            {
                _currentlySelectedLvi.SubItems[2].Text = speedString;
            }
        }

        private void PowerUsage_Leave(object sender, EventArgs e)
        {
            if (!CanEdit()) return;

            if (double.TryParse(field_PowerUsage.EntryText, out var value))
            {
                _currentlySelectedAlgorithm.PowerUsage = value;
            }
        }

        private void TextChangedExtraLaunchParameters(object sender, EventArgs e)
        {
            if (!CanEdit()) return;
            var extraLaunchParams = richTextBoxExtraLaunchParameters.Text.Replace("\r\n", " ");
            extraLaunchParams = extraLaunchParams.Replace("\n", " ");
            _currentlySelectedAlgorithm.ExtraLaunchParameters = extraLaunchParams;
        }

        #endregion

        //private void buttonBenchmark_Click(object sender, EventArgs e) {
        //    var device = new List<ComputeDevice>();
        //    device.Add(_computeDevice);
        //    var BenchmarkForm = new Form_Benchmark(
        //                BenchmarkPerformanceType.Standard,
        //                false, _currentlySelectedAlgorithm.NiceHashID);
        //    BenchmarkForm.ShowDialog();
        //    fieldBoxBenchmarkSpeed.EntryText = _currentlySelectedAlgorithm.BenchmarkSpeed.ToString();
        //    // update lvi speed
        //    if (_currentlySelectedLvi != null) {
        //        _currentlySelectedLvi.SubItems[2].Text = Helpers.FormatSpeedOutput(_currentlySelectedAlgorithm.BenchmarkSpeed);
        //    }
        //}
    }
}
