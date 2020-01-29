using MinerPlugin;
using NHM.Common;
using NHMCore.Configs;
using NHMCore.Mining.Plugins;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private string MinerDeviceName { get; set; }

        // mining algorithm stuff
        protected bool IsInit { get; private set; }

        public List<MiningPair> MiningPairs { get; private set; }

        public string GroupKey { get; protected set; } = "";

        CancellationTokenSource _endMiner;

        private bool IsUpdatingApi { get; set; } = false;

        private object _lock = new object();

        private Task _minerWatchdogTask;
        public Task MinerWatchdogTask
        {
            get
            {
                lock (_lock)
                {
                    return _minerWatchdogTask;
                }
            }
            set
            {
                lock (_lock)
                {
                    _minerWatchdogTask = value;
                }
            }
        }

        // Now every single miner is based from the Plugins
        private readonly PluginContainer _plugin;
        private readonly List<AlgorithmContainer> _algos;
        private readonly IMinerAsyncExtensions _miner;

        private readonly SemaphoreSlim _apiSemaphore = new SemaphoreSlim(1, 1);

        // you must use 
        protected Miner(PluginContainer plugin, List<MiningPair> miningPairs, string groupKey)
        {
            _plugin = plugin;
            // TODO this is now a must be of type IMinerAsyncExtensions
            _miner = _plugin.CreateMiner() as IMinerAsyncExtensions;

            // just so we can set algorithms states
            _algos = new List<AlgorithmContainer>();
            foreach (var pair in miningPairs)
            {
                var cDev = AvailableDevices.GetDeviceWithUuid(pair.Device.UUID);
                if (cDev == null) continue;
                var algoContainer = cDev.AlgorithmSettings.FirstOrDefault(a => a.Algorithm == pair.Algorithm);
                if (algoContainer == null) continue;
                _algos.Add(algoContainer);
            }

            MiningPairs = miningPairs;
            IsInit = MiningPairs != null && MiningPairs.Count > 0;
            GroupKey = groupKey;

            MinerDeviceName = plugin.PluginUUID;
            Logger.Info(MinerTag(), "NEW MINER CREATED");
        }

        // TAG for identifying miner
        private string MinerTag()
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


        private async Task<ApiData> GetSummaryAsync()
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
            MiningDataStats.UpdateGroup(apiData, _plugin.PluginUUID, _plugin.Name);

            return apiData;
        }

        private async Task<object> StartAsync(CancellationToken stop, string miningLocation, string username)
        {
            _miner.InitMiningLocationAndUsername(miningLocation, username);
            _miner.InitMiningPairs(MiningPairs);
            EthlargementIntegratedPlugin.Instance.Start(MiningPairs);
            var ret = await _miner.StartMiningTask(stop);
            var maxTimeout = _plugin.GetApiMaxTimeout(MiningPairs);
            MinerApiWatchdog.AddGroup(GroupKey, maxTimeout, DateTime.UtcNow);
            _algos.ForEach(a => a.IsCurrentlyMining = true);
            return ret;
        }

        private async Task StopAsync()
        {
            // TODO thing about this case, closing opening on switching
            EthlargementIntegratedPlugin.Instance.Stop(MiningPairs);
            MinerApiWatchdog.RemoveGroup(GroupKey);
            MiningDataStats.RemoveGroup(MiningPairs.Select(pair => pair.Device.UUID), _plugin.PluginUUID);
            await _miner.StopMiningTask();
            _algos.ForEach(a => a.IsCurrentlyMining = false);
            //if (_miner is IDisposable disposableMiner)
            //{
            //    disposableMiner.Dispose();
            //}
        }


        public Task StartMinerTask(CancellationToken stop, string miningLocation, string username)
        {
            var tsc = new TaskCompletionSource<object>();
            var wdTask = MinerWatchdogTask;
            if (wdTask == null || wdTask.IsCompleted)
            {
                MinerWatchdogTask = Task.Run(() => RunMinerWatchDogLoop(tsc, stop, miningLocation, username));
            }
            else
            {
                Logger.Error(MinerTag(), $"Trying to start an already started miner");
                tsc.SetResult("TODO error trying to start already started miner");
            }
            return tsc.Task;
        }

        private async Task RunMinerWatchDogLoop(TaskCompletionSource<object> tsc, CancellationToken stop, string miningLocation, string username)
        {
            try
            {
                var firstStart = true;
                using (_endMiner = new CancellationTokenSource())
                using (var linkedEndMiner = CancellationTokenSource.CreateLinkedTokenSource(stop, _endMiner.Token))
                {
                    Logger.Info(MinerTag(), $"Starting miner watchdog task");
                    while (!linkedEndMiner.IsCancellationRequested)
                    {
                        try
                        {
                            if (!firstStart)
                            {
                                Logger.Info(MinerTag(), $"Restart Mining in {MiningSettings.Instance.MinerRestartDelayMS}ms");
                            }
                            await TaskHelpers.TryDelay(MiningSettings.Instance.MinerRestartDelayMS, linkedEndMiner.Token);
                            var result = await StartAsync(linkedEndMiner.Token, miningLocation, username);
                            if (firstStart)
                            {
                                firstStart = false;
                                tsc.SetResult(result);
                            }
                            if (result is bool ok && ok)
                            {
                                var runningMinerTask = _miner.MinerProcessTask;
                                _ = MinerStatsLoop(runningMinerTask, linkedEndMiner.Token);
                                await runningMinerTask;
                                // TODO log something here
                                Logger.Info(MinerTag(), $"Running Miner Task Completed");
                            }
                            else
                            {
                                // TODO check if the miner file is missing or locked and blacklist the algorithm for a certain period of time 
                                Logger.Error(MinerTag(), $"StartAsync result: {result}");
                            }
                        }
                        catch (TaskCanceledException e)
                        {
                            Logger.Debug(MinerTag(), $"RunMinerWatchDogLoop TaskCanceledException: {e.Message}");
                            return;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(MinerTag(), $"RunMinerWatchDogLoop Exception: {e.Message}");
                        }
                    }
                }
            }
            finally
            {
                Logger.Info(MinerTag(), $"Exited miner watchdog");
            }
        }

        private async Task MinerStatsLoop(Task runningTask, CancellationToken stop)
        {
            try
            {
                // TODO make sure this interval is per miner plugin instead of a global one
                var minerStatusElapsedTimeChecker = new ElapsedTimeChecker(
                    () => TimeSpan.FromSeconds(MiningSettings.Instance.MinerAPIQueryInterval),
                    true);
                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                Func<bool> isOk = () => !runningTask.IsCompleted && !stop.IsCancellationRequested;
                Logger.Info(MinerTag(), $"MinerStatsLoop START");
                while (isOk())
                {
                    try
                    {
                        if (isOk()) await TaskHelpers.TryDelay(checkWaitTime, stop);
                        if (isOk() && minerStatusElapsedTimeChecker.CheckAndMarkElapsedTime()) await GetSummaryAsync();
                        
                        // check if stagnated and restart
                        var restartGroups = MinerApiWatchdog.GetTimedoutGroups(DateTime.UtcNow);
                        if (isOk() && (restartGroups?.Contains(GroupKey) ?? false))
                        {
                            Logger.Info(MinerTag(), $"Restarting miner group='{GroupKey}' API timestamp exceeded");
                            await StopAsync();
                            return;
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        Logger.Debug(MinerTag(), $"MinerStatsLoop TaskCanceledException: {e.Message}");
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(MinerTag(), $"Exception {e.Message}");
                    }
                }
            }
            finally
            {
                Logger.Info(MinerTag(), $"MinerStatsLoop END");
            }
        }

        public async Task StopTask()
        {
            try
            {
                _endMiner?.Cancel();
                await StopAsync();
            }
            catch (Exception e)
            {
                Logger.Info(MinerTag(), $"Stop: {e.Message}");
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
