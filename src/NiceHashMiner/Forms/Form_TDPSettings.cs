using System;
using System.Linq;
using System.Windows.Forms;
using NHM.DeviceMonitoring.TDP;
using NHMCore.Mining;

namespace NiceHashMiner.Forms
{
    public partial class Form_TDPSettings : Form
    {
        public Form_TDPSettings()
        {
            InitializeComponent();
            InitializeDevices();
            listViewDevicesTDP.ItemSelectionChanged += ListViewDevicesTDP_ItemSelectionChanged;
        }

        ComputeDevice _selectedComputeDevice = null;

        private void ListViewDevicesTDP_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            _selectedComputeDevice = e.Item.Tag as ComputeDevice;
            if (_selectedComputeDevice == null) return;
            var tdpMon = _selectedComputeDevice.DeviceMonitor as ITDP;
            labelSelectedDevice.Text = _selectedComputeDevice.GetFullName();
            labelSelectedTDPSetting.Text = $"Selected TDP Setting => {tdpMon.SettingType.ToString()}";
            // reset
            label_value_simple.Text = "";
            label_value_percentage.Text = "";
            label_value_raw.Text = "";
            textBox_simple.Text = "";
            textBox_percentage.Text = "";
            textBox_raw.Text = "";
        }

        private void InitializeDevices()
        {
            var devicesTDP = AvailableDevices.Devices.Where(dev => dev.DeviceMonitor != null && dev.DeviceMonitor is ITDP);
            listViewDevicesTDP.BeginUpdate();
            listViewDevicesTDP.Items.Clear();
            foreach (var computeDevice in devicesTDP)
            {
                var lvi = new ListViewItem
                {
                    Checked = computeDevice.Enabled,
                    Text = computeDevice.GetFullName(),
                    Tag = computeDevice
                };
                listViewDevicesTDP.Items.Add(lvi);
                if(computeDevice is ITDP tdp)
                {

                }
            }
            listViewDevicesTDP.EndUpdate();
            listViewDevicesTDP.Invalidate(true);
        }

        private void Btn_Refresh_Click(object sender, EventArgs e)
        {
            InitializeDevices();
        }

        private void Button_get_simple_Click(object sender, EventArgs e)
        {
            var tdp = _selectedComputeDevice?.DeviceMonitor as ITDP;
            if (tdp ==  null) return;
            label_value_simple.Text = tdp.TDPSimple.ToString();
        }

        private void Button_set_simple_Click(object sender, EventArgs e)
        {
            var tdp = _selectedComputeDevice?.DeviceMonitor as ITDP;
            if (tdp == null) return;
            var strType = textBox_simple.Text.ToUpper();
            if (Enum.TryParse(strType, out TDPSimpleType type))
            {
                tdp.SetTDPSimple(type);
            }
        }

        private void Button_get_percentage_Click(object sender, EventArgs e)
        {
            var tdp = _selectedComputeDevice?.DeviceMonitor as ITDP;
            if (tdp == null) return;
            label_value_percentage.Text = $"{tdp.TDPPercentage * 100}%";
        }

        private void Button_set_percentage_Click(object sender, EventArgs e)
        {
            var tdp = _selectedComputeDevice?.DeviceMonitor as ITDP;
            if (tdp == null) return;
            if (double.TryParse(textBox_percentage.Text, out var value))
            {
                var perc = value / 100.0;
                tdp.SetTDPPercentage(perc);
            }
        }

        private void Button_get_raw_Click(object sender, EventArgs e)
        {
            var tdp = _selectedComputeDevice?.DeviceMonitor as ITDP;
            if (tdp == null) return;
            label_value_raw.Text = tdp.TDPRaw.ToString();
        }

        private void Button_set_raw_Click(object sender, EventArgs e)
        {
            var tdp = _selectedComputeDevice?.DeviceMonitor as ITDP;
            if (tdp == null) return;
            if (double.TryParse(textBox_raw.Text, out var value))
            {
                tdp.SetTDPRaw(value);
            }
        }
    }
}
