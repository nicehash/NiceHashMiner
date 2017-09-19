using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Forms
{
    public partial class Form_DcriValues : Form
    {
        private const int INTENSITY      = 0;
        private const int SPEED          = 1;
        private const int SECONDARYSPEED = 2;
        private const int PROFIT         = 3;

        DualAlgorithm algorithm;
        bool isChange;
        bool isChangeSaved;
        int currentlySelectedIntensity = -1;

        public Form_DcriValues(DualAlgorithm algorithm) {
            InitializeComponent();
            this.algorithm = algorithm;
            algorithm.MakeIntensityBackup();

            initLocale();
            setIntensities();
            initializeControls();
            NiceHashStats.OnSMAUpdate += updateProfits;
        }

        private void initLocale() {
            listView_Intensities.Columns[INTENSITY].Text = International.GetText("Form_DcriValues_Intensity");
            listView_Intensities.Columns[SPEED].Text = International.GetText("AlgorithmsListView_Speed");
            listView_Intensities.Columns[SECONDARYSPEED].Text = International.GetText("Form_DcriValues_SecondarySpeed");
            listView_Intensities.Columns[PROFIT].Text = International.GetText("AlgorithmsListView_Rate");
            Text = International.GetText("Form_DcriValues_Title");
            button_Close.Text = International.GetText("Form_Settings_buttonCloseNoSaveText");
            button_Save.Text = International.GetText("Form_Settings_buttonSaveText");
        }

        private void setIntensities() {
            listView_Intensities.BeginUpdate();
            listView_Intensities.Items.Clear();
            foreach (var intensity in algorithm.AllIntensities) {
                ListViewItem lvi = new ListViewItem(intensity.ToString());
                
                lvi.SubItems.Add(algorithm.SpeedStringForIntensity(intensity));
                lvi.SubItems.Add(algorithm.SecondarySpeedStringForIntensity(intensity));
                lvi.SubItems.Add(algorithm.ProfitForIntensity(intensity).ToString("F8"));
                lvi.Tag = intensity;
                listView_Intensities.Items.Add(lvi);
            }
            listView_Intensities.EndUpdate();
        }

        private void initializeControls() {
            checkBox_TuningEnabled.Checked = algorithm.TuningEnabled;

            updateEnabled();

            field_TuningStart.EntryText = algorithm.TuningStart.ToString();
            field_TuningEnd.EntryText = algorithm.TuningEnd.ToString();
            field_TuningInterval.EntryText = algorithm.TuningInterval.ToString();

            field_Speed.SetOnTextChanged(textChangedSpeed);
            field_SecondarySpeed.SetOnTextChanged(textChangedSecondarySpeed);
            field_TuningStart.SetOnTextChanged(textChangedTuningStart);
            field_TuningEnd.SetOnTextChanged(textChangedTuningEnd);
            field_TuningInterval.SetOnTextChanged(textChangedTuningInterval);
        }

        private void updateEnabled() {
            listView_Intensities.Enabled = algorithm.TuningEnabled;
            field_TuningStart.Enabled = algorithm.TuningEnabled;
            field_TuningEnd.Enabled = algorithm.TuningEnabled;
            field_TuningInterval.Enabled = algorithm.TuningEnabled;
            if (!algorithm.TuningEnabled) {
                field_Speed.Enabled = false;
                field_SecondarySpeed.Enabled = false;
            }
        }

        #region Callback Events

        private void listView_Intensities_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
            var intensity = (int)e.Item.Tag;
            currentlySelectedIntensity = intensity;
            field_Speed.EntryText = algorithm.SpeedForIntensity(intensity).ToString();
            field_SecondarySpeed.EntryText = algorithm.SecondarySpeedForIntensity(intensity).ToString();

            field_Speed.Enabled = true;
            field_SecondarySpeed.Enabled = true;
        }

        private void button_Close_Clicked(object sender, EventArgs e) {
            isChangeSaved = false;
            this.Close();
        }
        private void button_Save_Clicked(object sender, EventArgs e) {
            isChangeSaved = true;
            this.Close();
        }
        private void checkBox_TuningEnabledCheckedChanged(object sender, EventArgs e) {
            isChange = true;

            algorithm.TuningEnabled = checkBox_TuningEnabled.Checked;
            updateEnabled();
        }

        private void textChangedSpeed(object sender, EventArgs e) {
            double value;
            if (Double.TryParse(field_Speed.EntryText, out value)) {
                isChange = true;
                algorithm.IntensitySpeeds[currentlySelectedIntensity] = value;
            }
            updateIntensities();
        }
        private void textChangedSecondarySpeed(object sender, EventArgs e) {
            double value;
            if (Double.TryParse(field_SecondarySpeed.EntryText, out value)) {
                isChange = true;
                algorithm.SecondaryIntensitySpeeds[currentlySelectedIntensity] = value;
            }
            updateIntensities();
        }
        private void textChangedTuningStart(object sender, EventArgs e) {
            int value;
            if (int.TryParse(field_TuningStart.EntryText, out value)) {
                isChange = true;
                algorithm.TuningStart = value;
            }
            updateIntensityList();
        }
        private void textChangedTuningEnd(object sender, EventArgs e) {
            int value;
            if (int.TryParse(field_TuningEnd.EntryText, out value)) {
                isChange = true;
                algorithm.TuningEnd = value;
            }
            updateIntensityList();
        }
        private void textChangedTuningInterval(object sender, EventArgs e) {
            int value;
            if (int.TryParse(field_TuningInterval.EntryText, out value)) {
                isChange = true;
                algorithm.TuningInterval = value;
            }
            updateIntensityList();
        }

        private void listView_Intensities_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                contextMenuStrip1.Items.Clear();
                var clearItem = new ToolStripMenuItem();
                clearItem.Text = International.GetText("AlgorithmsListView_ContextMenu_ClearItem");
                clearItem.Click += toolStripMenuItemClear_Click;
                contextMenuStrip1.Items.Add(clearItem);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        #endregion Callback Events

        private void updateIntensities() {
            foreach (ListViewItem lvi in listView_Intensities.Items) {
                var intensity = (int)lvi.Tag;
                lvi.SubItems[SPEED].Text = algorithm.SpeedStringForIntensity(intensity).ToString();
                lvi.SubItems[SECONDARYSPEED].Text = algorithm.SecondarySpeedStringForIntensity(intensity).ToString();
                lvi.SubItems[PROFIT].Text = algorithm.ProfitForIntensity(intensity).ToString("F8");
            }
        }

        private void updateIntensityList() {
            setIntensities();
        }

        private void updateProfits(object sender, EventArgs e) {
            foreach (ListViewItem lvi in listView_Intensities.Items) {
                var intensity = (int)lvi.Tag;
                lvi.SubItems[PROFIT].Text = algorithm.ProfitForIntensity(intensity).ToString("F8");
            }
        }

        private void toolStripMenuItemClear_Click(object sender, EventArgs e) {
            foreach (ListViewItem lvi in listView_Intensities.SelectedItems) {
                var intensity = (int)lvi.Tag;
                isChange = true;
                algorithm.IntensitySpeeds[intensity] = 0;
                algorithm.SecondaryIntensitySpeeds[intensity] = 0;
                updateIntensities();
            }
        }

        private void Form_DcriValues_FormClosing(object sender, FormClosingEventArgs e) {
            if (isChange && !isChangeSaved) {
                DialogResult result = MessageBox.Show(International.GetText("Form_Settings_buttonCloseNoSaveMsg"),
                                                      International.GetText("Form_Settings_buttonCloseNoSaveTitle"),
                                                      MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No) {
                    e.Cancel = true;
                    return;
                }
            }

            if (isChangeSaved) {
                algorithm.IntensityUpToDate = false;
                ConfigManager.CommitBenchmarks();
            } else {
                algorithm.RestoreIntensityBackup();
            }
        }
    }
}
