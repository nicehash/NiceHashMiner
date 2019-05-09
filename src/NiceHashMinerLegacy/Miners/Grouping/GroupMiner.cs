using NiceHashMiner.Configs;
using System.Collections.Generic;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.Grouping
{
    public class GroupMiner
    {
        public Miner Miner { get; protected set; }
        public string DevicesInfoString { get; }
        public AlgorithmType AlgorithmUUID { get; }
        public string Key { get; }
        public List<int> DevIndexes { get; }

        // , string miningLocation, string btcAdress, string worker
        public GroupMiner(List<MiningPair> miningPairs, string key)
        {
            AlgorithmUUID = AlgorithmType.NONE;
            DevicesInfoString = "N/A";
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
                    }
                    DevicesInfoString = "{ " + string.Join(", ", deviceNames) + " }";
                }
                // init miner
                {
                    Miner = MinerFactory.CreateMinerForMining(miningPairs);
                    if (Miner != null)
                    {
                        var mPair = miningPairs[0];
                        AlgorithmUUID = mPair.Algorithm.AlgorithmUUID;
                    }
                }
            }
        }

        public async Task Stop()
        {
            if (Miner != null && Miner.IsRunning)
            {
                Miner.Stop();
                // wait before going on
                await Task.Delay(ConfigManager.GeneralConfig.MinerRestartDelayMS);
            }
        }

        public void End()
        {
            Miner?.End();
        }

        public async Task Start(string miningLocation, string username)

        {
            if (Miner.IsRunning)
            {
                return;
            }
            // Wait before new start
            await Task.Delay(ConfigManager.GeneralConfig.MinerRestartDelayMS);
            Miner.Start(miningLocation, username);
        }
    }
}
