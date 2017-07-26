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
        public Form_DcriValues(Algorithm algorithm) {
            InitializeComponent();
            this.algorithm = algorithm;
            setIntensities();

            button_Close.MouseClick += button_Close_Clicked;
        }

        private void setIntensities() {
            listView_Intensities.BeginUpdate();
            listView_Intensities.Items.Clear();
            foreach (var intensity in algorithm.Intensities) {
                ListViewItem lvi = new ListViewItem(intensity.ToString());
                
                var speed = 0d;
                var secondarySpeed = 0d;
                algorithm.IntensitySpeeds.TryGetValue(intensity, out speed);
                algorithm.SecondaryIntensitySpeeds.TryGetValue(intensity, out secondarySpeed);
                lvi.SubItems.Add(Helpers.FormatSpeedOutput(speed) + "H/s");
                lvi.SubItems.Add(Helpers.FormatSpeedOutput(secondarySpeed) + "H/s");
                lvi.SubItems.Add(algorithm.ProfitForIntensity(intensity).ToString("F8"));
                lvi.Tag = intensity;
                listView_Intensities.Items.Add(lvi);
            }
            listView_Intensities.EndUpdate();
        }

        private void button_Close_Clicked(object sender, EventArgs e) {
            this.Close();
        }
        private void button_Save_Clicked(object sender, EventArgs e) {

        }
    }
}
