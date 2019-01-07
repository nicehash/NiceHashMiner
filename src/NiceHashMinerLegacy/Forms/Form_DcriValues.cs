using System;
using System.Windows.Forms;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMiner.Stats;

namespace NiceHashMiner.Forms
{
    public partial class FormDcriValues : Form
    {
        private const int Intensity = 0;
        private const int Speed = 1;
        private const int Secondaryspeed = 2;
        private const int Profit = 3;
        private const int Power = 4;
        private bool _isChange;

        private bool _isInitFinished;

        private readonly DualAlgorithm _algorithm;
        private int _currentlySelectedIntensity = -1;
        private bool _isChangeSaved;

        public FormDcriValues(DualAlgorithm algorithm)
        {
            InitializeComponent();
            _algorithm = algorithm;
            algorithm.MakeIntensityBackup();

            InitLocale();
            SetIntensities();
            InitializeControls();
            NiceHashStats.OnSmaUpdate += UpdateProfits;

            _isInitFinished = true;
        }

        private bool IsChange
        {
            get => _isChange;
            set => _isChange = _isChange || _isInitFinished && value;
        }

        private void InitLocale()
        {
            listView_Intensities.Columns[Intensity].Text = International.GetText("Form_DcriValues_Intensity");
            listView_Intensities.Columns[Speed].Text = International.GetText("AlgorithmsListView_Speed");
            listView_Intensities.Columns[Secondaryspeed].Text = International.GetText("Form_DcriValues_SecondarySpeed");
            listView_Intensities.Columns[Profit].Text = International.GetText("AlgorithmsListView_Rate");
            listView_Intensities.Columns[Power].Text = International.GetText("Form_DcriValues_Power");
            Text = International.GetText("Form_DcriValues_Title");
            button_Close.Text = International.GetText("Form_Settings_buttonCloseNoSaveText");
            button_Save.Text = International.GetText("Form_Settings_buttonSaveText");
            checkBox_TuningEnabled.Text = International.GetText("Form_DcriValues_TuningEnabled");

            field_Speed.InitLocale(toolTip1,
                International.GetText("Form_Settings_Algo_BenchmarkSpeed") + ":",
                International.GetText("Form_Settings_ToolTip_AlgoBenchmarkSpeed"));
            field_SecondarySpeed.InitLocale(toolTip1,
                International.GetText("Form_Settings_Algo_SecondaryBenchmarkSpeed") + ":",
                International.GetText("Form_Settings_ToolTip_AlgoSecondaryBenchmarkSpeed"));
            field_TuningStart.InitLocale(toolTip1,
                International.GetText("Form_DcriValues_TuningStart") + ":",
                International.GetText("Form_DcriValues_ToolTip_TuningStart"));
            field_TuningEnd.InitLocale(toolTip1,
                International.GetText("Form_DcriValues_TuningEnd") + ":",
                International.GetText("Form_DcriValues_ToolTip_TuningEnd"));
            field_TuningInterval.InitLocale(toolTip1,
                International.GetText("Form_DcriValues_TuningInterval") + ":",
                International.GetText("Form_DcriValues_ToolTip_TuningInterval"));
            field_Power.InitLocale(toolTip1,
                International.GetText("Form_Settings_Algo_PowerUsage") + ":",
                International.GetText("Form_Settings_ToolTip_PowerUsage"));

            toolTip1.SetToolTip(checkBox_TuningEnabled, International.GetText("Form_DcriValues_ToolTip_TuningEnabled"));
            toolTip1.SetToolTip(pictureBox_TuningEnabled,
                International.GetText("Form_DcriValues_ToolTip_TuningEnabled"));
        }

        private void SetIntensities()
        {
            listView_Intensities.BeginUpdate();
            listView_Intensities.Items.Clear();
            foreach (var intensity in _algorithm.AllIntensities)
            {
                var lvi = new ListViewItem(intensity.ToString());
                _algorithm.IntensityPowers.TryGetValue(intensity, out var power);

                lvi.SubItems.Add(_algorithm.SpeedStringForIntensity(intensity));
                lvi.SubItems.Add(_algorithm.SecondarySpeedStringForIntensity(intensity));
                lvi.SubItems.Add(_algorithm.ProfitForIntensity(intensity).ToString("F8"));
                lvi.SubItems.Add(power.ToString("F2"));
                lvi.Tag = intensity;
                listView_Intensities.Items.Add(lvi);
            }

            listView_Intensities.EndUpdate();
        }

        private void InitializeControls()
        {
            checkBox_TuningEnabled.Checked = _algorithm.TuningEnabled;

            UpdateEnabled();

            field_TuningStart.EntryText = _algorithm.TuningStart.ToString();
            field_TuningEnd.EntryText = _algorithm.TuningEnd.ToString();
            field_TuningInterval.EntryText = _algorithm.TuningInterval.ToString();

            field_Speed.SetOnTextChanged(TextChangedSpeed);
            field_SecondarySpeed.SetOnTextChanged(TextChangedSecondarySpeed);
            field_TuningStart.SetOnTextChanged(TextChangedTuningStart);
            field_TuningEnd.SetOnTextChanged(TextChangedTuningEnd);
            field_TuningInterval.SetOnTextChanged(TextChangedTuningInterval);
            field_Power.SetOnTextChanged(TextChangedPower);
        }

        private void UpdateEnabled()
        {
            listView_Intensities.Enabled = _algorithm.TuningEnabled;
            field_TuningStart.Enabled = _algorithm.TuningEnabled;
            field_TuningEnd.Enabled = _algorithm.TuningEnabled;
            field_TuningInterval.Enabled = _algorithm.TuningEnabled;

            if (!_algorithm.TuningEnabled)
            {
                field_Speed.Enabled = false;
                field_SecondarySpeed.Enabled = false;
                field_Power.Enabled = false;
            }
        }

        private void UpdateIntensities()
        {
            foreach (ListViewItem lvi in listView_Intensities.Items)
            {
                var intensity = (int) lvi.Tag;
                _algorithm.IntensityPowers.TryGetValue(intensity, out var power);

                lvi.SubItems[Speed].Text = _algorithm.SpeedStringForIntensity(intensity);
                lvi.SubItems[Secondaryspeed].Text = _algorithm.SecondarySpeedStringForIntensity(intensity);
                lvi.SubItems[Profit].Text = _algorithm.ProfitForIntensity(intensity).ToString("F8");
                lvi.SubItems[Power].Text = power.ToString("F2");
            }
        }

        private void UpdateIntensityList()
        {
            SetIntensities();
        }

        private void UpdateProfits(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView_Intensities.Items)
            {
                var intensity = (int) lvi.Tag;
                lvi.SubItems[Profit].Text = _algorithm.ProfitForIntensity(intensity).ToString("F8");
            }
        }

        private void ToolStripMenuItemClear_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView_Intensities.SelectedItems)
            {
                var intensity = (int) lvi.Tag;
                IsChange = true;
                _algorithm.IntensitySpeeds[intensity] = 0;
                _algorithm.SecondaryIntensitySpeeds[intensity] = 0;
                UpdateIntensities();
            }
        }

        private void Form_DcriValues_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsChange && !_isChangeSaved)
            {
                var result = MessageBox.Show(International.GetText("Form_Settings_buttonCloseNoSaveMsg"),
                    International.GetText("Form_Settings_buttonCloseNoSaveTitle"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (_isChangeSaved)
            {
                _algorithm.IntensityUpToDate = false;
                ConfigManager.CommitBenchmarks();
            }
            else
            {
                _algorithm.RestoreIntensityBackup();
            }
        }

        #region Callback Events

        private void ListView_Intensities_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var intensity = (int) e.Item.Tag;
            _currentlySelectedIntensity = intensity;
            _isInitFinished = false;
            field_Speed.EntryText = _algorithm.SpeedForIntensity(intensity).ToString();
            field_SecondarySpeed.EntryText = _algorithm.SecondarySpeedForIntensity(intensity).ToString();
            _algorithm.IntensityPowers.TryGetValue(intensity, out var power);
            field_Power.EntryText = power.ToString();
            _isInitFinished = true;

            field_Speed.Enabled = true;
            field_SecondarySpeed.Enabled = true;
            field_Power.Enabled = true;
        }

        private void Button_Close_Clicked(object sender, EventArgs e)
        {
            _isChangeSaved = false;
            Close();
        }

        private void Button_Save_Clicked(object sender, EventArgs e)
        {
            _isChangeSaved = true;
            Close();
        }

        private void CheckBox_TuningEnabledCheckedChanged(object sender, EventArgs e)
        {
            IsChange = true;

            _algorithm.TuningEnabled = checkBox_TuningEnabled.Checked;
            UpdateEnabled();
        }

        private void TextChangedSpeed(object sender, EventArgs e)
        {
            if (double.TryParse(field_Speed.EntryText, out var value))
            {
                IsChange = true;
                _algorithm.IntensitySpeeds[_currentlySelectedIntensity] = value;
            }

            UpdateIntensities();
        }

        private void TextChangedSecondarySpeed(object sender, EventArgs e)
        {
            if (double.TryParse(field_SecondarySpeed.EntryText, out var value))
            {
                IsChange = true;
                _algorithm.SecondaryIntensitySpeeds[_currentlySelectedIntensity] = value;
            }

            UpdateIntensities();
        }

        private void TextChangedPower(object sender, EventArgs e)
        {
            if (double.TryParse(field_Power.EntryText, out var value))
            {
                IsChange = true;
                _algorithm.IntensityPowers[_currentlySelectedIntensity] = value;
            }

            UpdateIntensities();
        }

        private void TextChangedTuningStart(object sender, EventArgs e)
        {
            if (int.TryParse(field_TuningStart.EntryText, out var value))
            {
                IsChange = true;
                _algorithm.TuningStart = value;
            }

            UpdateIntensityList();
        }

        private void TextChangedTuningEnd(object sender, EventArgs e)
        {
            if (int.TryParse(field_TuningEnd.EntryText, out var value))
            {
                IsChange = true;
                _algorithm.TuningEnd = value;
            }

            UpdateIntensityList();
        }

        private void TextChangedTuningInterval(object sender, EventArgs e)
        {
            if (int.TryParse(field_TuningInterval.EntryText, out var value))
            {
                IsChange = true;
                _algorithm.TuningInterval = value;
            }

            UpdateIntensityList();
        }

        private void ListView_Intensities_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Items.Clear();
                var clearItem = new ToolStripMenuItem
                {
                    Text = International.GetText("AlgorithmsListView_ContextMenu_ClearItem")
                };
                clearItem.Click += ToolStripMenuItemClear_Click;
                contextMenuStrip1.Items.Add(clearItem);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        #endregion Callback Events
    }
}
