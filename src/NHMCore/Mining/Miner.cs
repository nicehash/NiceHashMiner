using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using NHM.Common;
using NHMCore.Configs;
using NHMCore.Mining.Plugins;

namespace NHMCore.Mining
{
    public class Miner
    {
        public static Miner CreateMinerForMining(List<MiningPair> miningPairs, string groupKey)
        {
            var pair = miningPairs.FirstOrDefault();
            if (pair == null || pair.Algorithm == null) return null;
            var algorithm = pair.Algorithm;
            var plugin = MinerPluginsManager.GetPluginWithUuid(algorithm.MinerID);
            if (plugin != null)
            {
                return new Miner(plugin, miningPairs, groupKey);
            }
            return null;
        }

        // used to identify miner instance
        protected readonly long MinerID;

        private string _minerTag;
        public string MinerDeviceName { get; set; }

        // mining algorithm stuff
        protected bool IsInit { get; private set; }

        public List<MiningPair> MiningPairs { get; protected set; }

        public bool IsRunning { get; protected set; } = false;
        public string GroupKey { get; protected set; } = "";

        CancellationTokenSource EndMiner { get; } = new CancellationTokenSource();
        protected bool _isEnded { get; private set; }

        public bool IsUpdatingApi { get; protected set; } = false;

        // Now every single miner is based from the Plugins
        private readonly PluginContainer _plugin;
        private readonly IMiner _miner;

        private readonly SemaphoreSlim _apiSemaphore = new SemaphoreSlim(1, 1);

        // you must use 
        protected Miner(PluginContainer plugin, List<MiningPair> miningPairs, string groupKey)
        {
            _plugin = plugin;
            _miner = _plugin.CreateMiner();

            MiningPairs = miningPairs;
            IsInit = MiningPairs != null && MiningPairs.Count > 0;
            GroupKey = groupKey;

            MinerDeviceName = plugin.PluginUUID;
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

        public async Task<ApiData> GetSummaryAsync()
        {
            var apiData = new ApiData();
            if (!IsUpdatingApi)
            {
                IsUpdatingApi = true;
                await _apiSemaphore.WaitAsync();
                try
                {
                    apiData = await _miner.GetMinerStatsDataAsync();
                }
                catch (Exception e)
                {
                    Logger.Error(MinerTag(), $"GetSummaryAsync error: {e.Message}");
                }
                finally
                {
                    IsUpdatingApi = false;
                    _apiSemaphore.Release();
                }
            }

            UpdateApiTimestamp(apiData);

            // TODO workaround plugins should return this info
            // create empty stub if it is null
            if (apiData == null)
            {
                Logger.Debug(MinerTag(), "GetSummary returned null... Will create ZERO fallback");
                apiData = new ApiData();
            }
            if (apiData.AlgorithmSpeedsPerDevice == null)
            {
                apiData = new ApiData();
                var perDevicePowerDict = new Dictionary<string, int>();
                var perDeviceSpeedsDict = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDeviceSpeeds = MiningPairs.Select(pair => (pair.Device.UUID, pair.Algorithm.IDs.Select(type => new AlgorithmTypeSpeedPair(type, 0d))));
                foreach (var kvp in perDeviceSpeeds)
                {
                    var uuid = kvp.Item1; // kvp.UUID compiler doesn't recognize ValueTypes lib???
                    perDeviceSpeedsDict[uuid] = kvp.Item2.ToList();
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.AlgorithmSpeedsPerDevice = perDeviceSpeedsDict;
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
                apiData.AlgorithmSpeedsTotal = perDeviceSpeedsDict.First().Value;
            }
            else if (apiData.AlgorithmSpeedsPerDevice != null && apiData.PowerUsagePerDevice.Count == 0)
            {
                var perDevicePowerDict = new Dictionary<string, int>();
                foreach (var kvp in MiningPairs)
                {
                    var uuid = kvp.Device.UUID;
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
            }

            // TODO temporary here move it outside later
            MiningStats.UpdateGroup(apiData, _plugin.PluginUUID, _plugin.Name);

            return apiData;
        }

        // TODO this thing 
        public void Start(string miningLocation, string username)
        {
            if (_isEnded) return;
            _miner.InitMiningLocationAndUsername(miningLocation, username);
            _miner.InitMiningPairs(MiningPairs);
            EthlargementIntegratedPlugin.Instance.Start(MiningPairs);
            _miner.StartMining();
            IsRunning = true;
            // maxTimeout = ConfigManager.GeneralConfig.CoolDownCheckEnabled
            var maxTimeout = _plugin.GetApiMaxTimeout(MiningPairs);
            MinerApiWatchdog.AddGroup(GroupKey, maxTimeout, DateTime.UtcNow);
        }

        public void Stop()
        {
            // TODO thing about this case, closing opening on switching
            // EthlargementIntegratedPlugin.Instance.Stop(_miningPairs);
            MinerApiWatchdog.RemoveGroup(GroupKey);
            MiningStats.RemoveGroup(MiningPairs.Select(pair => pair.Device.UUID), _plugin.PluginUUID);
            IsRunning = false;
            _miner.StopMining();
            //if (_miner is IDisposable disposableMiner)
            //{
            //    disposableMiner.Dispose();
            //}
        }

        // TODO check cleanup here
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


        #region MinerApiWatchdog
        private double _lastSpeedsTotalSum = 0d;
        private double _lastPerDevSpeedsTotalSum = 0d;

        // TODO this can be moved in MinerApiWatchdog
        private void UpdateApiTimestamp(ApiData apiData)
        {
            // we will not update api timestamps if we have no data or speeds are zero
            if (apiData == null)
            {
                // TODO debug log no api data
                return;
            }
            if (apiData.AlgorithmSpeedsTotal == null && apiData.AlgorithmSpeedsPerDevice == null)
            {
                // TODO debug log cannot get speeds
                return;
            }
            var speedsTotalSum = apiData.AlgorithmSpeedsTotal?.Select(p => p.Speed).Sum() ?? 0d;
            var perDevSpeedsTotalSum = apiData.AlgorithmSpeedsPerDevice?.Values.SelectMany(pl => pl).Select(p => p.Speed).Sum() ?? 0d;
            if (speedsTotalSum == 0d && perDevSpeedsTotalSum == 0d)
            {
                // TODO debug log speeds are zero
                return;
            }
            if (speedsTotalSum == _lastSpeedsTotalSum && perDevSpeedsTotalSum == _lastPerDevSpeedsTotalSum)
            {
                // TODO debug log speeds seem to be stuck
                return;
            }
            // update 
            _lastSpeedsTotalSum = speedsTotalSum;
            _lastPerDevSpeedsTotalSum = perDevSpeedsTotalSum;
            MinerApiWatchdog.UpdateApiTimestamp(GroupKey, DateTime.UtcNow);
        }
        #endregion MinerApiWatchdog
    }
}
