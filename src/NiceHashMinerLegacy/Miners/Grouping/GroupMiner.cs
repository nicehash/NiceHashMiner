using NiceHashMiner.Configs;
using System.Collections.Generic;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NiceHashMiner.Miners.Grouping
{
    public class GroupMiner
    {
        CancellationTokenSource EndMiner { get; } = new CancellationTokenSource();
        public Miner Miner { get; protected set; }
        public string Key { get; }
        public List<int> DevIndexes { get; }

        // , string miningLocation, string btcAdress, string worker
        public GroupMiner(List<MiningPair> miningPairs, string key)
        {
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
                }
                // init miner
                {
                    Miner = MinerFactory.CreateMinerForMining(miningPairs);
                    if (Miner != null)
                    {
                        var mPair = miningPairs[0];
                    }
                }
            }
        }

        public async Task Stop()
        {
            try
            {
                if (Miner == null) return;
                if (!Miner.IsRunning) return;
                //if (EndMiner.IsCancellationRequested) return;
                Miner.Stop();
                // wait before going on
                await Task.Delay(ConfigManager.GeneralConfig.MinerRestartDelayMS, EndMiner.Token);
            }
            catch(Exception e)
            {
                Logger.Info("GROUP_MINER", $"Stop: {e.Message}");
            }
            
        }

        public void End()
        {
            try
            {
                EndMiner.Cancel();
            }
            catch { }
            Miner?.End();
        }

        public async Task Start(string miningLocation, string username)
        {
            try
            {
                if (Miner == null) return;
                if (Miner.IsRunning) return;
                if (EndMiner.IsCancellationRequested) return;
                // Wait before new start
                await Task.Delay(ConfigManager.GeneralConfig.MinerRestartDelayMS, EndMiner.Token);
                if (EndMiner.IsCancellationRequested) return;
                Miner.Start(miningLocation, username);
            }
            catch (Exception e)
            {
                Logger.Info("GROUP_MINER", $"Start: {e.Message}");
            }
            
        }
    }
}
