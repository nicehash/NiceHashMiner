using NiceHashMiner;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinerSmokeTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.dgv_algo.Rows.Clear();
            this.dgv_devices.Rows.Clear();

            dgv_devices.CellContentClick += dgv_devices_CellContentClick;
            dgv_algo.CellContentClick += dgv_algo_CellContentClick;

            this.Shown += new EventHandler(this.FormShown);
        }

        public static object[] GetDeviceRowData(ComputeDevice d)
        {
            object[] rowData = { d.Enabled, d.ID, d.GetFullName() };
            return rowData;
        }

        public static object[] GetAlgorithmRowData(Algorithm a)
        {
            object[] rowData = { a.Enabled, a.AlgorithmName, a.MinerBaseTypeName };
            return rowData;
        }

        private async void FormShown(object sender, EventArgs e)
        {
            MinerPaths.InitializePackages();
            ConfigManager.GeneralConfig.Use3rdPartyMiners = Use3rdPartyMiners.YES;
            await ComputeDeviceManager.QueryDevicesAsync();
            var devices = AvailableDevices.Devices;

            foreach(var device in devices)
            {
                dgv_devices.Rows.Add(GetDeviceRowData(device));

                var newRow = dgv_devices.Rows[dgv_devices.Rows.Count - 1];
                newRow.Tag = device;
            }
        }


        private void dgv_devices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dgv_algo.Rows.Clear();
            if (!(e.RowIndex >= 0)) return;

            var senderGrid = (DataGridView)sender;
            var row = senderGrid.Rows[e.RowIndex];

            ComputeDevice device;
            if (row.Tag is ComputeDevice dev) {
                device = dev;
            } else
            {
                // TAG is not device type
                return;
            }

            var cellItem = row.Cells[e.ColumnIndex];
            if (cellItem is DataGridViewCheckBoxCell checkbox)
            {
                var deviceEnabled = checkbox.Value != null && (bool)checkbox.Value;
                checkbox.Value = !deviceEnabled;
                device.Enabled = !deviceEnabled;
            }
            var algorithms = device.GetAlgorithmSettings();
            foreach (var algo in algorithms)
            {
                dgv_algo.Rows.Add(GetAlgorithmRowData(algo));

                var newRow = dgv_algo.Rows[dgv_algo.Rows.Count - 1];
                newRow.Tag = algo;
            }
        }

        private void dgv_algo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!(e.RowIndex >= 0)) return;

            var senderGrid = (DataGridView)sender;
            var row = senderGrid.Rows[e.RowIndex];

            Algorithm algo;
            if (row.Tag is Algorithm a)
            {
                algo = a;
            }
            else
            {
                // TAG is not algo type
                return;
            }

            var cellItem = row.Cells[e.ColumnIndex];
            if (cellItem is DataGridViewCheckBoxCell checkbox)
            {
                var deviceEnabled = checkbox.Value != null && (bool)checkbox.Value;
                checkbox.Value = !deviceEnabled;
                algo.Enabled = !deviceEnabled;
            }
        }

        private async void btn_startTest_Click(object sender, EventArgs e)
        {
            var enabledDevs = AvailableDevices.Devices.Where(dev => dev.Enabled);
            foreach (var device in enabledDevs)
            {
                var enabledAlgorithms = device.GetAlgorithmSettings().Where(algo => algo.Enabled);
                foreach (var algorithm in enabledAlgorithms)
                {
                    try {
                        var miner = NiceHashMiner.Miners.MinerFactory.CreateMiner(device.DeviceType, algorithm);

                        List<MiningPair> pair = new List<MiningPair>();
                        pair.Add(new MiningPair(device, algorithm));
                        MiningSetup setup = new MiningSetup(pair);

                        miner.InitMiningSetup(setup);
                        var url = StratumHelpers.GetLocationUrl(algorithm.NiceHashID, "eu", miner.ConectionType);

                        tbx_info.Text += $"TESTING: path: {algorithm.MinerBinaryPath}, miner: {algorithm.MinerBaseTypeName}, algorithm: {algorithm.AlgorithmName}" + Environment.NewLine;
                        lbl_status.Text = "TESTING";
                        lbl_minerName.Text = algorithm.MinerBaseTypeName;
                        lbl_testingMinerVersion.Text = "unknown";
                        lbl_algoName.Text = algorithm.AlgorithmName;

                        miner.Start(url, Globals.DemoUser, "test");

                        await Task.Delay(1000 * 30);
                        miner.Stop();
                        await Task.Delay(1000 * 5);

                        lbl_status.Text = "NOT TESTING";
                        lbl_minerName.Text = "";
                        lbl_testingMinerVersion.Text = "";
                        lbl_algoName.Text = "";
                        tbx_info.Text += $"FINISHED: path: {algorithm.MinerBinaryPath}, miner: {algorithm.MinerBaseTypeName}, algorithm: {algorithm.AlgorithmName}" + Environment.NewLine;
                        tbx_info.Text += $"FINISHED: path: {algorithm.MinerBinaryPath}, miner: {algorithm.MinerBaseTypeName}, algorithm: {algorithm.AlgorithmName}" + Environment.NewLine;
                    } catch (Exception ex)
                    {
                        tbx_info.Text += $"Exception {ex}" + Environment.NewLine;
                    }
                } 
            }
        }
    }
}
