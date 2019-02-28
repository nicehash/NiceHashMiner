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
            this.Shown += new EventHandler(this.FormShown);
        }

        public static object[] GetDeviceRowData(ComputeDevice d)
        {
            object[] rowData = { d.Enabled, d.ID, d.GetFullName() };
            return rowData;
        }

        public static object[] GetAlgorithmRowData(Algorithm a)
        {
            object[] rowData = { a.Enabled, a.AlgorithmName };
            return rowData;
        }

        private async void FormShown(object sender, EventArgs e)
        {
            MinerPaths.InitializePackages();
            await ComputeDeviceManager.QueryDevicesAsync();
            var devices = AvailableDevices.Devices;
            dgv_algo.Columns.Add("algoEnabled", "Enabled");
            dgv_algo.Columns.Add("algoName", "Name");

            foreach(var device in devices)
            {
                dgv_devices.Rows.Add(GetDeviceRowData(device));

                var newRow = dgv_devices.Rows[dgv_devices.Rows.Count - 2];
                newRow.Tag = device.Name;
            }
        }


        private void dgv_devices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dgv_algo.Rows.Clear();

            var senderGrid = (DataGridView)sender;
            var row = senderGrid.Rows[e.RowIndex];

            var devices = AvailableDevices.Devices;
            foreach(var device in devices)
            {
                if (device.Name == (string)row.Tag)
                {
                    var algorithms = device.GetAlgorithmSettings();
                    foreach(var algo in algorithms)
                    {
                        dgv_algo.Rows.Add(GetAlgorithmRowData(algo));
                    }
                }

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
