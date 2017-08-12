using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public partial class Form_DcriValues : Form
    {
        Algorithm algorithm;
        bool isChange;
        public Form_DcriValues(Algorithm algorithm) {
            InitializeComponent();
            this.algorithm = algorithm;
            setIntensities();
            initializeControls();
        }

        private void setIntensities() {
            listView_Intensities.BeginUpdate();
            listView_Intensities.Items.Clear();
            foreach (var intensity in algorithm.Intensities) {
                ListViewItem lvi = new ListViewItem(intensity.ToString());
                
                lvi.SubItems.Add(Helpers.FormatSpeedOutput(algorithm.SpeedForIntensity(intensity)) + "H/s");
                lvi.SubItems.Add(Helpers.FormatSpeedOutput(algorithm.SecondarySpeedForIntensity(intensity)) + "H/s");
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
            field_Speed.EntryText = algorithm.SpeedForIntensity(intensity).ToString();
            field_SecondarySpeed.EntryText = algorithm.SecondarySpeedForIntensity(intensity).ToString();

            field_Speed.Enabled = true;
            field_Speed.Enabled = false;
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

        #endregion Callback Events
    }
}
