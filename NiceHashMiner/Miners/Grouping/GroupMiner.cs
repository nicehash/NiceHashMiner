using NiceHashMiner.Configs;
using System.Collections.Generic;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Grouping
{
    public class GroupMiner
    {
        public Miner Miner { get; protected set; }
        public string DevicesInfoString { get; }
        public AlgorithmType AlgorithmType { get; }

        public AlgorithmType DualAlgorithmType { get; }

        // for now used only for dagger identification AMD or NVIDIA
        public DeviceType DeviceType { get; }

        public double CurrentRate { get; set; }
        public string Key { get; }
        public List<int> DevIndexes { get; }

        public double TotalPower { get; }

        // , string miningLocation, string btcAdress, string worker
        public GroupMiner(List<MiningPair> miningPairs, string key)
        {
            AlgorithmType = AlgorithmType.NONE;
            DualAlgorithmType = AlgorithmType.NONE;
            DevicesInfoString = "N/A";
            CurrentRate = 0;
            Key = key;
            if (miningPairs.Count > 0)
            {
                // sort pairs by device id
                miningPairs.Sort((a, b) => a.Device.ID - b.Device.ID);
                // init name scope and IDs
                {
                    var deviceNames = new List<string>();
                    DevIndexes = new List<int>();
                    foreach (var pair in miningPairs)
                    {
                        deviceNames.Add(pair.Device.NameCount);
                        DevIndexes.Add(pair.Device.Index);
                        TotalPower += pair.Algorithm.PowerUsage;
                    }
                    DevicesInfoString = "{ " + string.Join(", ", deviceNames) + " }";
                }
                // init miner
                {
                    var mPair = miningPairs[0];
                    DeviceType = mPair.Device.DeviceType;
                    Miner = MinerFactory.CreateMiner(mPair.Device, mPair.Algorithm);
                    if (Miner != null)
                    {
                        Miner.InitMiningSetup(new MiningSetup(miningPairs));
                        AlgorithmType = mPair.Algorithm.NiceHashID;
                        DualAlgorithmType = mPair.Algorithm.DualNiceHashID;
                    }
                }
            }
        }

        public void Stop()
        {
            if (Miner != null && Miner.IsRunning)
            {
                Miner.Stop();
                // wait before going on
                System.Threading.Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);
            }
            CurrentRate = 0;
        }

        public void End()
        {
            Miner?.End();
            CurrentRate = 0;
        }

        public void Start(string miningLocation, string btcAdress, string worker)
        {
            if (Miner.IsRunning)
            {
                return;
            }
            // Wait before new start
            System.Threading.Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);
            var locationUrl = Globals.GetLocationUrl(AlgorithmType, miningLocation, Miner.ConectionType);
            Miner.Start(locationUrl, btcAdress, worker);
        }
    }
}
