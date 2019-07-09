using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Interfaces.StateSetters;
using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common.Enums;

using static NiceHashMiner.Translations;

namespace NiceHashMiner.Forms.Components
{
    public partial class DevicesMainBoard : UserControl, IEnabledDeviceStateSetter, IDevicesStateDisplayer
    {
        private enum Column : int
        {
            Enabled = 0,
            Name,
            Status,
            Temperature,
            Load,
            RPM,
            StartStop,
            AlgorithmsOptions,
            //PowerModeDropdown // disable for now
        }

        public event EventHandler<(string uuid, bool enabled)> SetDeviceEnabledState;

        //public static object[] GetRowData()
        //{
        //    const string status = "Pending";
        //    object[] row0 = { true, "Name", status, "Temperature", "Load", "RPM", "Start/Stop" };
        //    return row0;
        //}

        private static string buttonLabel(DeviceState state) {
            // assume disabled
            var buttonLabel = "N/A";
            if (state == DeviceState.Stopped)
            {
                buttonLabel = "Start";
            }
            else if (state == DeviceState.Mining || state == DeviceState.Benchmarking)
            {
                buttonLabel = "Stop";
            }
            return Tr(buttonLabel);
        }

        private static string stateStr(DeviceState state) {
            return Tr(state.ToString());
        }

        private static string getAlgosStats(ComputeDevice d) {
            var allAlgos = d.AlgorithmSettings;
            var enabledAlgos = allAlgos.Count(a => a.Enabled);
            var benchmarkedAlgos = allAlgos.Count(a => !a.BenchmarkNeeded);
            return $"{allAlgos.Count} / {enabledAlgos} / {benchmarkedAlgos}";
        }

        private static string numStr(int num) {
            if (num < 0) {
                return Tr("N/A");
            }
            return Tr(num.ToString());
        }

        public static object[] GetRowData(ComputeDevice d) {
            
            object[] rowData = { d.Enabled, d.GetFullName(), stateStr(d.State), numStr((int)d.Temp), numStr((int)d.Load), numStr(d.FanSpeed), buttonLabel(d.State), getAlgosStats(d) };
            return rowData;
        }

        // TODO enable this when combobox is working
        //private enum PowerMode : int
        //{
        //    Low = 0,
        //    Medium,
        //    High
        //}

        public DevicesMainBoard()
        {
            InitializeComponent();
            devicesDataGridView.CellContentClick += DevicesDataGridView_CellContentClick;
        }

        private void SetRowColumnItemValue(DataGridViewRow row, Column col, object value)
        {
            var cellItem = row.Cells[(int)col];
            cellItem.Value = value;
        }

        private void DevicesDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            Console.WriteLine($"RowIndex {e.RowIndex} ColumnIndex {e.ColumnIndex}");

            if (!(e.RowIndex >= 0)) return;

            //var columnItem = senderGrid.Columns[e.ColumnIndex];
            var row = senderGrid.Rows[e.RowIndex];
            var deviceUUID = (string)row.Tag;
            Console.WriteLine($"Row TAG {row.Tag}");
            var cellItem = row.Cells[e.ColumnIndex];
            switch (cellItem)
            {
                case DataGridViewButtonCell button:
                    var dev = AvailableDevices.GetDeviceWithUuidOrB64Uuid(deviceUUID);
                    if (dev == null) return;
                    if (dev.State == DeviceState.Stopped) {
                        button.Value = Tr("Starting");
                        ApplicationStateManager.StartDevice(dev);
                    } else if (dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking) {
                        button.Value = Tr("Stopping");
                        ApplicationStateManager.StopDevice(dev);
                    }
                    Console.WriteLine("DataGridViewButtonCell button");
                    break;
                case DataGridViewCheckBoxCell checkbox:
                    var deviceEnabled = checkbox.Value != null && (bool)checkbox.Value;
                    checkbox.Value = !deviceEnabled;
                    SetDeviceEnabledState?.Invoke(null, (deviceUUID, !deviceEnabled));
                    break;
                // TODO not working
                //case DataGridViewComboBoxCell comboBox:
                //    Console.WriteLine($"DataGridViewComboBoxCell comboBox {comboBox.Value}");
                //    break;

            }
        }

        // TODO this one does everything for now
        void IDevicesStateDisplayer.RefreshDeviceListView(object sender, EventArgs _)
        {
            FormHelpers.SafeInvoke(this, () => {
                // see what devices to 
                // iterate each row
                var devicesToAddUuids = new List<string>();
                var allDevs = AvailableDevices.Devices;
                foreach (var dev in allDevs)
                {
                    bool found = false;
                    // can't LINQ Where on rows??
                    foreach (DataGridViewRow row in devicesDataGridView.Rows)
                    {
                        var tagUUID = (string)row.Tag;
                        if (tagUUID == dev.Uuid)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) devicesToAddUuids.Add(dev.Uuid);
                }

                // filter what to add if any
                var devsToAdd = AvailableDevices.Devices.Where(dev => devicesToAddUuids.Contains(dev.Uuid));
                foreach (var dev in devsToAdd)
                {
                    // add dummy data
                    devicesDataGridView.Rows.Add(GetRowData(dev));
                    // add tag
                    var newRow = devicesDataGridView.Rows[devicesDataGridView.Rows.Count - 1];
                    newRow.Tag = dev.Uuid;
                }
                // update or init states
                foreach (DataGridViewRow row in devicesDataGridView.Rows)
                {
                    var tagUUID = (string)row.Tag;
                    var dev = AvailableDevices.Devices.FirstOrDefault(d => d.Uuid == tagUUID);
                    SetRowColumnItemValue(row, Column.Enabled, dev.Enabled);
                    SetRowColumnItemValue(row, Column.Name, dev.GetFullName());
                    SetRowColumnItemValue(row, Column.Status, stateStr(dev.State));
                    SetRowColumnItemValue(row, Column.Temperature, numStr((int)dev.Temp));
                    SetRowColumnItemValue(row, Column.Load, numStr((int)dev.Load));
                    SetRowColumnItemValue(row, Column.RPM, numStr(dev.FanSpeed));
                    SetRowColumnItemValue(row, Column.StartStop, buttonLabel(dev.State));
                    SetRowColumnItemValue(row, Column.AlgorithmsOptions, getAlgosStats(dev));
                }
            });
        }
    }
}
