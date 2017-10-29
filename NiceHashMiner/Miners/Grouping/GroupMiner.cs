﻿using System;
using System.Collections.Generic;
using System.Text;
using NiceHashMiner.Enums;
using NiceHashMiner.Devices;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Miners.Grouping {
    public class GroupMiner {
        public Miner Miner { get; protected set; }
        public string DevicesInfoString { get; private set; }
        public string DevicesDetailedInfoString { get; private set; }
        public AlgorithmType AlgorithmType { get; private set; }
        public AlgorithmType DualAlgorithmType { get; private set; }
        // for now used only for dagger identification AMD or NVIDIA
        public DeviceType DeviceType { get; private set; }
        public double CurrentRate { get; set; }
        public string Key { get; private set; }
        public List<int> DevIndexes { get; private set; }

        // , string miningLocation, string btcAdress, string worker
        public GroupMiner(List<MiningPair> miningPairs, string key) {
            AlgorithmType = AlgorithmType.NONE;
            DualAlgorithmType = AlgorithmType.NONE;
            DevicesInfoString = "N/A";
            DevicesDetailedInfoString = "N/A";
            CurrentRate = 0;
            Key = key;
            if (miningPairs.Count > 0) {
                // sort pairs by device id
                miningPairs.Sort((a, b) => a.Device.ID - b.Device.ID);
                // init name scope and IDs
                {
                    List<string> deviceNames = new List<string>();
                    List<string> deviceNamesWithInfo = new List<string>();
                    DevIndexes = new List<int>();
                    foreach (var pair in miningPairs) {
                        deviceNames.Add(pair.Device.NameCount);
                        deviceNamesWithInfo.Add(pair.Device.NameCount + " (" 
                            + ((int)(pair.Device.Load)).ToString() + "%" 
                            + (pair.Device.Temp == 0 ? "" : ", " + pair.Device.Temp.ToString() + "˚C")
                            + (pair.Device.FanSpeed == 0 ? "" : ", " + pair.Device.FanSpeed.ToString() + "rpm")
                             + ")");
                        DevIndexes.Add(pair.Device.Index);
                    }
                    DevicesInfoString = "{ " + String.Join(", ", deviceNames) + " }";
                    DevicesDetailedInfoString = "{ " + String.Join(", ", deviceNamesWithInfo) + " }";
                }
                // init miner
                {
                    var mPair = miningPairs[0];
                    DeviceType = mPair.Device.DeviceType;
                    Miner = MinerFactory.CreateMiner(mPair.Device, mPair.Algorithm);
                    if(Miner != null) {
                        Miner.InitMiningSetup(new MiningSetup(miningPairs));
                        AlgorithmType = mPair.Algorithm.NiceHashID;
                        DualAlgorithmType = mPair.Algorithm.DualNiceHashID;
                    }
                }
            }
        }

        public void Stop() {
            if (Miner != null && Miner.IsRunning) {
                Miner.Stop(MinerStopType.SWITCH);
                // wait before going on
                System.Threading.Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);
            }
            CurrentRate = 0;
        }

        public void End() {
            if (Miner != null) {
                Miner.End();
            }
            CurrentRate = 0;
        }

        public void Start(string miningLocation, string btcAdress, string worker) {
            if(Miner.IsRunning) {
                return;
            }
            // Wait before new start
            System.Threading.Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);
            string locationURL = Globals.GetLocationURL(AlgorithmType, miningLocation, Miner.ConectionType);
            Miner.Start(locationURL, btcAdress, worker);
        }
    }
}
