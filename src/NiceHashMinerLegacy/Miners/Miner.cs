using NiceHashMiner.Miners.Grouping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinerPlugin;
using NiceHashMinerLegacy.Common;
using System.Threading;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;

namespace NiceHashMiner
{
    public abstract class Miner
    {
        // used to identify miner instance
        protected readonly long MinerID;

        private string _minerTag;
        public string MinerDeviceName { get; set; }

        // mining algorithm stuff
        protected bool IsInit { get; private set; }

        public List<MiningPair> MiningPairs { get; protected set; }

        public bool IsRunning { get; protected set; } = false;
        public string GroupKey { get; protected set; } = "";
        public List<int> DevIndexes { get; private set; } = new List<int>();

        CancellationTokenSource EndMiner { get; } = new CancellationTokenSource();
        protected bool _isEnded { get; private set; }

        public bool IsUpdatingApi { get; protected set; } = false;


        protected Miner(string minerDeviceName, List<MiningPair> miningPairs, string groupKey)
        {
            MiningPairs = miningPairs;
            IsInit = MiningPairs != null && MiningPairs.Count > 0;
            if (IsInit)
            {
                foreach (var pair in miningPairs)
                {
                    // for PRODUCTION we still need these indexes get rid of this when possible
                    var cDev = AvailableDevices.GetDeviceWithUuidOrB64Uuid(pair.Device.UUID);
                    var index = cDev?.Index ?? -1;
                    if (index < 0) continue;
                    DevIndexes.Add(index);
                }
            }

            GroupKey = groupKey;

            MinerDeviceName = minerDeviceName;
            Logger.Info(MinerTag(), "NEW MINER CREATED");
        }

        // TAG for identifying miner
        public string MinerTag()
        {
            if (_minerTag == null)
            {
                const string mask = "{0}-MINER_ID({1})-DEVICE_IDs({2})";
                // no devices set
                if (!IsInit)
                {
                    return string.Format(mask, MinerDeviceName, MinerID, "NOT_SET");
                }

                // contains ids
                var ids = MiningPairs.Select(cdevs => cdevs.Device.ID.ToString()).ToList();
                _minerTag = string.Format(mask, MinerDeviceName, MinerID, string.Join(",", ids));
            }

            return _minerTag;
        }

        public abstract void Start(string miningLocation, string username);
        public abstract Task<ApiData> GetSummaryAsync();
        public abstract void Stop();

        public void End()
        {
            try
            {
                if (EndMiner.IsCancellationRequested) return;
                _isEnded = true;
                EndMiner.Cancel();
            }
            catch (Exception e)
            {
                Logger.Info(MinerTag(), $"End: {e.Message}");
            }
            finally
            {
                Logger.Info(MinerTag(), $"Setting End and Stopping");
                Stop();
            }
        }

        public async Task StopTask()
        {
            try
            {
                if (!IsRunning) return;
                Logger.Debug(MinerTag(), "BEFORE Stopping");
                Stop();
                Logger.Debug(MinerTag(), "AFTER Stopping");
                if (EndMiner.IsCancellationRequested) return;
                // wait before going on // TODO state right here
                await Task.Delay(ConfigManager.GeneralConfig.MinerRestartDelayMS, EndMiner.Token);
            }
            catch (Exception e)
            {
                Logger.Info(MinerTag(), $"Stop: {e.Message}");
            }
        }

        public async Task StartTask(string miningLocation, string username)
        {
            var startCalled = false;
            try
            {
                if (IsRunning) return;
                if (EndMiner.IsCancellationRequested) return;
                // Wait before new start
                await Task.Delay(ConfigManager.GeneralConfig.MinerRestartDelayMS, EndMiner.Token);
                if (EndMiner.IsCancellationRequested) return;
                Logger.Debug(MinerTag(), "BEFORE Starting");
                Start(miningLocation, username);
                startCalled = true;
                Logger.Debug(MinerTag(), "AFTER Starting");
            }
            catch (Exception e)
            {
                Logger.Info(MinerTag(), $"Start: {e.Message}");
            }
            finally
            {
                var stopOrEndCalled = startCalled && (EndMiner.IsCancellationRequested || _isEnded);
                if (stopOrEndCalled)
                {
                    Stop();
                }
            }
        }
    }
}
