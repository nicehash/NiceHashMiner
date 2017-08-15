using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace NiceHashMiner.Forms
{
    public partial class Form_DcriValues : Form
    {
        Algorithm algorithm;
        bool isChange;
        int currentlySelectedIntensity = -1;
        public Form_DcriValues(Algorithm algorithm) {
            InitializeComponent();
            this.algorithm = algorithm;
            setIntensities();
            initializeControls();
        }

        private void setIntensities() {
            listView_Intensities.BeginUpdate();
            listView_Intensities.Items.Clear();
            foreach (var intensity in algorithm.AllIntensities) {
                ListViewItem lvi = new ListViewItem(intensity.ToString());
                
                lvi.SubItems.Add(algorithm.SpeedStringForIntensity(intensity));
                lvi.SubItems.Add(algorithm.SpeedStringForIntensity(intensity));
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
            this.Close();
        }
        private void button_Save_Clicked(object sender, EventArgs e) {

        }
        private void checkBox_TuningEnabledCheckedChanged(object sender, EventArgs e) {
            isChange = true;

            algorithm.TuningEnabled = checkBox_TuningEnabled.Checked;
            updateEnabled();
        }

        private void textChangedSpeed(object sender, EventArgs e) {
            double value;
            if (Double.TryParse(field_Speed.EntryText, out value)) {
                algorithm.IntensitySpeeds[currentlySelectedIntensity] = value;
            }
        }
        private void textChangedSecondarySpeed(object sender, EventArgs e) {
            double value;
            if (Double.TryParse(field_SecondarySpeed.EntryText, out value)) {
                algorithm.SecondaryIntensitySpeeds[currentlySelectedIntensity] = value;
            }
        }
        private void textChangedTuningStart(object sender, EventArgs e) {
            int value;
            if (int.TryParse(field_TuningStart.EntryText, out value)) {
                algorithm.TuningStart = value;
            }
            updateTunings();
        }
        private void textChangedTuningEnd(object sender, EventArgs e) {
            int value;
            if (int.TryParse(field_TuningEnd.EntryText, out value)) {
                algorithm.TuningEnd = value;
            }
            updateTunings();
        }
        private void textChangedTuningInterval(object sender, EventArgs e) {
            int value;
            if (int.TryParse(field_TuningInterval.EntryText, out value)) {
                algorithm.TuningInterval = value;
            }
            updateTunings();
        }

        #endregion Callback Events

        private void updateTunings() {

        }
    }
}
