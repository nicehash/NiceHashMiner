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
            listView_Intensities.Columns[Intensity].Text = Translations.Tr("Intensity");
            listView_Intensities.Columns[Speed].Text = Translations.Tr("Speed");
            listView_Intensities.Columns[Secondaryspeed].Text = Translations.Tr("Secondary Speed");
            listView_Intensities.Columns[Profit].Text = Translations.Tr("BTC/Day");
            listView_Intensities.Columns[Power].Text = Translations.Tr("Power");
            Text = Translations.Tr("ClaymoreDual Tuning");
            button_Close.Text = Translations.Tr("&Close without Saving");
            button_Save.Text = Translations.Tr("&Save and Close");
            checkBox_TuningEnabled.Text = Translations.Tr("Tuning Enabled");

            field_Speed.InitLocale(toolTip1,
                Translations.Tr("Benchmark Speed (H/s)") + ":",
                Translations.Tr("Fine tune algorithm ratios by manually setting benchmark speeds for each algorithm."));
            field_SecondarySpeed.InitLocale(toolTip1,
                Translations.Tr("Secondary Benchmark Speed (H/s)") + ":",
                Translations.Tr("Speed for the secondary algorithm when using dual algo mining."));
            field_TuningStart.InitLocale(toolTip1,
                Translations.Tr("Tuning Start") + ":",
                Translations.Tr("The first dcri value to use for tuning"));
            field_TuningEnd.InitLocale(toolTip1,
                Translations.Tr("Tuning End") + ":",
                Translations.Tr("The last dcri value to use for tuning"));
            field_TuningInterval.InitLocale(toolTip1,
                Translations.Tr("Tuning Interval") + ":",
                Translations.Tr("The interval for dcri values to use for tuning"));
            field_Power.InitLocale(toolTip1,
                Translations.Tr("Power Usage (W)") + ":",
                Translations.Tr("The power used by this algorithm in Watts.\n Algorithm profits will deduct power costs when this and electricity cost are above 0."));

            toolTip1.SetToolTip(checkBox_TuningEnabled, Translations.Tr("If enabled, NHML will benchmark through all listed dcri values and store the speeds.\nNHML will then use the most profitable speed combination for mining.\nThis mode ignores the -dcri Extra Launch Paramater."));
            toolTip1.SetToolTip(pictureBox_TuningEnabled,
                Translations.Tr("If enabled, NHML will benchmark through all listed dcri values and store the speeds.\nNHML will then use the most profitable speed combination for mining.\nThis mode ignores the -dcri Extra Launch Paramater."));
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
                var result = MessageBox.Show(Translations.Tr("Warning! You are choosing to close settings without saving. Are you sure you would like to continue?"),
                    Translations.Tr("Warning!"),
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
                    Text = Translations.Tr("Clear Algorithm Speed")
                };
                clearItem.Click += ToolStripMenuItemClear_Click;
                contextMenuStrip1.Items.Add(clearItem);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        #endregion Callback Events
    }
}
