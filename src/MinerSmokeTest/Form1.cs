using NiceHashMiner;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
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

            //if(lv_minerList.Items.Count == 0)
            //{
            //    MessageBox.Show("Miner list is empty.\nPlease add miners!");
            //}
            //else
            //{


            //    foreach (var minerExe in lv_minerList.Items)
            //    {
            //        //vzami checkbox za izbiro minerja pa vse not zafilaj
            //        var miner = new NiceHashMiner.Miners.GMiner();

            //        foreach (var device in devices)
            //        {
            //            MinerPaths.InitializePackages();
            //            ConfigManager.GeneralConfig.Use3rdPartyMiners = Use3rdPartyMiners.YES;
            //            var algorithmList = device.GetAlgorithmSettings();

            //            foreach (var algo in algorithmList)
            //            {
            //                if (algo.MinerBaseType != MinerBaseType.GMiner || algo.AlgorithmName != "ZHash")
            //                {
            //                    continue;
            //                } else
            //                {
            //                    List<MiningPair> pairs = new List<MiningPair>();
            //                    MiningPair pair = new MiningPair(device, algo);
            //                    pairs.Add(pair);

            //                    MiningSetup setup = new MiningSetup(pairs);
            //                    miner.InitMiningSetup(setup);

            //                    var url = StratumHelpers.GetLocationUrl(algo.NiceHashID, "eu", miner.ConectionType);
            //                    miner.Start(url, Globals.DemoUser, "test");

            //                    Thread.Sleep(10 * 1000);
            //                    miner.Stop();

            //                    Thread.Sleep(10 * 1000);
            //                }


            //            }

            //        }



            //    }
            //}
        }

    }
}
