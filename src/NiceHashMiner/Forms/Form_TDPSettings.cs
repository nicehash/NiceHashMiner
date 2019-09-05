using NHM.DeviceDetection;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.TDP;
using NiceHashMiner.Mining;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public partial class Form_TDPSettings : Form
    {
        private struct TDPSettings
        {
            string Mode { get; set; }
            double Value { get; set; }
        }
        private Dictionary<string, List<TDPSettings>> DeviceTDPSettings;

        public Form_TDPSettings()
        {
            InitializeComponent();
            InitializeDevices();
        }

        private async void InitializeDevices()
        {
            if (AvailableDevices.Devices.Count == 0)
            {
                await Task.Delay(1000 * 10);
            }
            var detectionResult = DeviceDetection.DetectionResult;
            var monitors = await DeviceMonitorManager.GetDeviceMonitors(AvailableDevices.Devices.Select(d => d.BaseDevice), detectionResult.IsDCHDriver);
            var devices = new List<ComputeDevice>();
            foreach (var monitor in monitors.Where(mon => mon is ITDP))
            {
                devices.Add(AvailableDevices.GetDeviceWithUuid(monitor.UUID));
            }
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var computeDevice in devices)
            {
                var lvi = new ListViewItem
                {
                    Checked = computeDevice.Enabled,
                    Text = computeDevice.GetFullName(),
                    Tag = computeDevice
                };
                listView1.Items.Add(lvi);
                if(computeDevice is ITDP tdp)
                {

                }
            }
            listView1.EndUpdate();
            listView1.Invalidate(true);
        }

        private void Btn_Refresh_Click(object sender, EventArgs e)
        {
            InitializeDevices();
        }

        private void Btn_SaveData_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                var config = 
                var selectedDevice = listView1.SelectedItems[0].Tag as ComputeDevice;


                selectedDevice.SetDeviceConfig();
            } else
            {
                MessageBox.Show("Please select an item before assigning a value.");
            }
        }
    }
}
