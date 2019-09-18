using System;
using System.Windows.Forms;
using NHM.Common.Enums;
using NHMCore;
using NHMCore.Mining;

namespace NiceHashMiner.Forms.Components
{
    public partial class AlgorithmSettingsControl : UserControl, AlgorithmsListView.IAlgorithmsListView
    {
        private ComputeDevice _computeDevice;
        private AlgorithmContainer _currentlySelectedAlgorithm;
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
            groupBoxSelectedAlgorithmSettings.Text = string.Format(Translations.Tr("Selected Algorithm: {0}"),
                Translations.Tr("NONE"));
            Enabled = false;
            fieldBoxBenchmarkSpeed.EntryText = "";
            secondaryFieldBoxBenchmarkSpeed.EntryText = "";
            field_PowerUsage.EntryText = "";
            richTextBoxExtraLaunchParameters.Text = "";
        }

        public void InitLocale(ToolTip toolTip1)
        {
            field_PowerUsage.InitLocale(toolTip1,
                Translations.Tr("Power Usage (W)") + ":",
                Translations.Tr("The power used by this algorithm in Watts.\n Algorithm profits will deduct power costs when this and electricity cost are above 0."));
            fieldBoxBenchmarkSpeed.InitLocale(toolTip1,
                Translations.Tr("Benchmark Speed") + ":",
                Translations.Tr("Fine tune algorithm ratios by manually setting benchmark speeds for each algorithm."));
            secondaryFieldBoxBenchmarkSpeed.InitLocale(toolTip1,
                Translations.Tr("Secondary Benchmark Speed") + ":",
                Translations.Tr("Speed for the secondary algorithm when using dual algo mining."));
            toolTip1.SetToolTip(groupBoxExtraLaunchParameters,
                Translations.Tr("Additional launch parameters when launching miner and this algorithm."));
            toolTip1.SetToolTip(pictureBox1, Translations.Tr("Additional launch parameters when launching miner and this algorithm."));
        }

        private static string ParseStringDefault(string value)
        {
            return value ?? "";
        }

        private static string ParseDoubleDefault(double value)
        {
            return value <= 0 ? "" : value.ToString();
        }


        public bool IsInBenchmark { get; set; } = false;

        public void SetCurrentlySelected(ListViewItem lvi, ComputeDevice computeDevice)
        {
            // should not happen ever
            if (lvi == null) return;

            _computeDevice = computeDevice;
            if (lvi.Tag is AlgorithmContainer algorithm)
            {
                _selected = true;
                _currentlySelectedAlgorithm = algorithm;
                _currentlySelectedLvi = lvi;
                Enabled = lvi.Checked && !IsInBenchmark;

                var selectedAlgoName = $"{algorithm.AlgorithmName} ({algorithm.MinerBaseTypeName})";
                groupBoxSelectedAlgorithmSettings.Text = string.Format(
                    Translations.Tr("Selected Algorithm: {0}"), ""); // keep the translation
                labelSelectedAlgorithm.Text = selectedAlgoName;

                field_PowerUsage.EntryText = ParseDoubleDefault(algorithm.PowerUsage);
                var unit = algorithm.IDs[0].GetUnitPerSecond();
                fieldBoxBenchmarkSpeed.LabelText = Translations.Tr("Benchmark Speed") + $" ({unit}):";
                fieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(algorithm.BenchmarkSpeed);
                richTextBoxExtraLaunchParameters.Text = ParseStringDefault(algorithm.ExtraLaunchParameters);
                if (algorithm.IsDual) 
                {
                    var secondaryUnit = algorithm.IDs[1].GetUnitPerSecond();
                    secondaryFieldBoxBenchmarkSpeed.LabelText = Translations.Tr("Secondary Benchmark Speed") + $" ({secondaryUnit}):";
                    secondaryFieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(algorithm.SecondaryBenchmarkSpeed);
                    secondaryFieldBoxBenchmarkSpeed.Enabled = true;
                } 
                else 
                {
                    secondaryFieldBoxBenchmarkSpeed.LabelText = Translations.Tr("Secondary Benchmark Speed") + ":";
                    secondaryFieldBoxBenchmarkSpeed.EntryText = "";
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
                if (lvi.Tag is AlgorithmContainer algorithm)
                {
                    fieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(algorithm.BenchmarkSpeed);
                    field_PowerUsage.EntryText = ParseDoubleDefault(algorithm.PowerUsage);
                    if (algorithm.IsDual) 
                    {
                        secondaryFieldBoxBenchmarkSpeed.EntryText = ParseDoubleDefault(algorithm.SecondaryBenchmarkSpeed);
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
                && _currentlySelectedAlgorithm.IsDual)
            {
                _currentlySelectedAlgorithm.SecondaryBenchmarkSpeed = secondaryValue;
            }
            UpdateSpeedText();
        }

        private void UpdateSpeedText()
        {
            var speedString = _currentlySelectedAlgorithm.BenchmarkSpeedString();
            // update lvi speed
            if (_currentlySelectedLvi != null)
            {
                var speedIndex = (int)AlgorithmsListView.Column.SPEEDS;
                _currentlySelectedLvi.SubItems[speedIndex].Text = speedString;
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
    }
}
