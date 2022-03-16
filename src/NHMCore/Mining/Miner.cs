using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHMCore.Configs;
using NHMCore.Mining.Plugins;
using NHMCore.Notifications;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHM.DeviceMonitoring;

namespace NHMCore.Mining
{
    public class Miner
    {
        public static Miner CreateMinerForMining(List<AlgorithmContainer> algorithms, string groupKey)
        {
            try
            {
                // Assert that all the plugins are from the same plugin container
                var plugin = algorithms.First().PluginContainer;
                if (algorithms.Any(algo => plugin != algo.PluginContainer)) throw new Exception("Different Algorithms PluginContainers. Grouping logic broken.");
                return new Miner(plugin, algorithms, groupKey);
            }
            catch (Exception e)
            {
                Logger.Error("NHMCore.Mining", $"CreateMinerForMining error: {e}");
                return null;
            }
        }

        // used to identify miner instance
        protected readonly long MinerID;

        private string _minerTag;
        private string MinerDeviceName { get; set; }

        // mining algorithm stuff
        protected bool IsInit { get; private set; }

        private List<MiningPair> _miningPairs { get; set; }

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
        private readonly IMiner _miner;

        private readonly SemaphoreSlim _apiSemaphore = new SemaphoreSlim(1, 1);

        // you must use 
        protected Miner(PluginContainer plugin, List<AlgorithmContainer> algorithms, string groupKey)
        {
            _plugin = plugin;
            _miner = _plugin.CreateMiner();

            // just so we can set algorithms states
            _algos = algorithms;
            _miningPairs = algorithms.Select(algo => algo.ToMiningPair()).ToList();
            IsInit = _miningPairs != null && _miningPairs.Any();
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
                var ids = _miningPairs.Select(cdevs => cdevs.Device.ID.ToString()).ToList();
                _minerTag = string.Format(mask, MinerDeviceName, MinerID, string.Join(",", ids));
            }

            return _minerTag;
        }

        private enum ApiDataStatus
        {
            NULL,
            ALGO_SPEEDS_NULL,
            OK,
            NEGATIVE_SPEEDS,
            ABNORMAL_SPEEDS,
            OK_MISSING_DEVICE,
            TEN_PERCENT_ABOVE_OR_BELOW,
        }

        private static ApiDataStatus ExamineApiData(ApiData apiData, List<MiningPair> miningPairs)
        {
            if (apiData == null) return ApiDataStatus.NULL;
            if (apiData.AlgorithmSpeedsPerDevice == null) return ApiDataStatus.ALGO_SPEEDS_NULL;


            var anyNegative = apiData.AlgorithmSpeedsPerDevice.Any(apiDev => apiDev.Value.Any(kvp => kvp.speed < 0));
            if (anyNegative) return ApiDataStatus.NEGATIVE_SPEEDS;

            var miningPairAndReportedSpeedsPairs = apiData.AlgorithmSpeedsPerDevice.Select(p => (mp: miningPairs.FirstOrDefault(mp => mp.Device.UUID == p.Key), speeds: p.Value.Select(s => s.speed).ToArray()))
                .ToArray();

            var andDeviceMissing = miningPairAndReportedSpeedsPairs.Any(p => p.mp == null);
            var hashDiffers = false;
            foreach (var (mp, speeds) in miningPairAndReportedSpeedsPairs)
            {
                for (var i = 0; i < Math.Min(speeds.Length, mp.Algorithm.Speeds.Count); i++)
                {
                    if (speeds[i] < (0.9 * mp.Algorithm.Speeds[i]) && speeds[i] != 0)
                    {
                        AvailableNotifications.CreateWarningHashrateDiffers(mp, "lower");
                        Logger.Warn("Miner", "Hashrate was too low on " + mp.Device.Name);
                        hashDiffers = true;
                    }
                    else if (speeds[i] > (4 * mp.Algorithm.Speeds[i]))
                    {
                        AvailableNotifications.CreateErrorExtremeHashrate(mp);
                        Logger.Error("Miner", "Hashrate was abnormal on " + mp.Device.Name);
                        return ApiDataStatus.ABNORMAL_SPEEDS;
                    }
                    else if (speeds[i] > (1.1 * mp.Algorithm.Speeds[i]))
                    {
                        AvailableNotifications.CreateWarningHashrateDiffers(mp, "higher");
                        Logger.Warn("Miner", "Hashrate was too high on " + mp.Device.Name);
                        hashDiffers = true;
                    }
                }
            }
            if (hashDiffers) return ApiDataStatus.TEN_PERCENT_ABOVE_OR_BELOW;

            // API data all good
            return ApiDataStatus.OK;
        }

        private async Task GetSummaryAsync()
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
                    if (apiData.AlgorithmSpeedsPerDevice != null)
                    {
                        var anyNegative = apiData.AlgorithmSpeedsPerDevice.Any(apiDev => apiDev.Value.Any(kvp => kvp.speed < 0));
                        if (anyNegative) await StopAsync();
                    }
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
                var perDeviceSpeedsDict = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDeviceSpeeds = _miningPairs.Select(pair => (pair.Device.UUID, pair.Algorithm.IDs.Select(type => (type, 0d))));
                foreach (var kvp in perDeviceSpeeds)
                {
                    var uuid = kvp.Item1; // kvp.UUID compiler doesn't recognize ValueTypes lib???
                    perDeviceSpeedsDict[uuid] = kvp.Item2.ToList();
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.AlgorithmSpeedsPerDevice = perDeviceSpeedsDict;
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
            }
            else if (apiData.AlgorithmSpeedsPerDevice != null && apiData.PowerUsagePerDevice.Count == 0)
            {
                var perDevicePowerDict = new Dictionary<string, int>();
                foreach (var kvp in _miningPairs)
                {
                    var uuid = kvp.Device.UUID;
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
            }
           
            // TODO temporary here move it outside later
            MiningDataStats.UpdateGroup(apiData, _plugin.PluginUUID, _plugin.Name);

            var apiDataStatus = ExamineApiData(apiData, _miningPairs);
            
            if (apiDataStatus != ApiDataStatus.OK)
            {
                if (apiDataStatus == ApiDataStatus.ABNORMAL_SPEEDS)
                {
                    _ = _miner.StopMiningTask();
                    Logger.Info("Miner", "Miner stopped due to extreme hash");
                }
                return;
            }
        }

        private async Task<object> StartAsync(CancellationToken stop, string username)
        {
            _miner.InitMiningLocationAndUsername("auto", username);
            _miner.InitMiningPairs(_miningPairs);
            EthlargementIntegratedPlugin.Instance.Start(_miningPairs);
            var ret = await _miner.StartMiningTask(stop);
            var maxTimeout = _plugin.GetApiMaxTimeout(_miningPairs);
            MinerApiWatchdog.AddGroup(GroupKey, maxTimeout, DateTime.UtcNow);
            _algos.ForEach(a => a.IsCurrentlyMining = true);
            _algos.ForEach(a => a.ComputeDevice.State = DeviceState.Mining);
            return ret;
        }

        private async Task StopAsync()
        {
            // TODO thing about this case, closing opening on switching
            EthlargementIntegratedPlugin.Instance.Stop(_miningPairs);
            MinerApiWatchdog.RemoveGroup(GroupKey);
            MiningDataStats.RemoveGroup(_miningPairs.Select(pair => pair.Device.UUID), _plugin.PluginUUID);
            await _miner.StopMiningTask();
            _algos.ForEach(a => a.IsCurrentlyMining = false);
            //if (_miner is IDisposable disposableMiner)
            //{
            //    disposableMiner.Dispose();
            //}
        }


        public Task StartMinerTask(CancellationToken stop, string username)
        {
            var tsc = new TaskCompletionSource<object>();
            var wdTask = MinerWatchdogTask;
            if (wdTask == null || wdTask.IsCompleted)
            {
                MinerWatchdogTask = Task.Run(() => RunMinerWatchDogLoop(tsc, stop, username));
            }
            else
            {
                Logger.Error(MinerTag(), $"Trying to start an already started miner");
                tsc.SetResult("TODO error trying to start already started miner");
            }
            return tsc.Task;
        }

        private async Task RunMinerWatchDogLoop(TaskCompletionSource<object> tsc, CancellationToken stop, string username)
        {
            // if we fail 3 times in a row under certain conditions mark on of them
            const int maxRestartCount = 3;
            int restartCount = 0;
            const int minRestartTimeInSeconds = 240;
            try
            {
                var firstStart = true;
                using (_endMiner = new CancellationTokenSource())
                using (var linkedEndMiner = CancellationTokenSource.CreateLinkedTokenSource(stop, _endMiner.Token))
                {
                    Logger.Info(MinerTag(), $"Starting miner watchdog task");
                    while (!linkedEndMiner.IsCancellationRequested && (restartCount < maxRestartCount))
                    {
                        var startTime = DateTime.UtcNow;
                        try
                        {
                            if (!firstStart)
                            {
                                Logger.Info(MinerTag(), $"Restart Mining in {MiningSettings.Instance.MinerRestartDelayMS}ms");
                                AvailableNotifications.CreateRestartedMinerInfo(DateTime.UtcNow.ToLocalTime(), _plugin.Name);
                                await TaskHelpers.TryDelay(MiningSettings.Instance.MinerRestartDelayMS, linkedEndMiner.Token);
                            }
                            var result = await StartAsync(linkedEndMiner.Token, username);
                            if (firstStart)
                            {
                                firstStart = false;
                                tsc.SetResult(result);
                            }
                            if (result is bool ok && ok)
                            {
                                var runningMinerTask = _miner.MinerProcessTask;
                                if (_algos.Any(a => a.AlgorithmName == "RandomXmonero")) _ = XMRingStartedWaitTime(runningMinerTask, linkedEndMiner.Token);
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
                        finally
                        {
                            var endTime = DateTime.UtcNow;
                            var elapsedSeconds = (endTime - startTime).TotalSeconds - (MiningSettings.Instance.MinerRestartDelayMS/1000);
                            if (elapsedSeconds < minRestartTimeInSeconds)
                            {
                                restartCount++;
                            }
                            else
                            {
                                restartCount = 0;
                            }
                            if (restartCount >= maxRestartCount)
                            {
                                var firstAlgo = _algos.FirstOrDefault();
                                Random randWait = new Random();
                                firstAlgo.IgnoreUntil = DateTime.UtcNow.AddMinutes(randWait.Next(20, 30));
                                await MiningManager.MinerRestartLoopNotify();
                            }
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

        private async Task XMRingStartedWaitTime(Task runningTask, CancellationToken stop)
        {
            try
            {
                var checkWaitTime = TimeSpan.FromMilliseconds(60000);
                Func<bool> isOk = () => !runningTask.IsCompleted && !stop.IsCancellationRequested;
                Logger.Info(MinerTag(), $"XMRingStartedWaitTime START");
                try
                {
                    DeviceMonitorManager.CloseComputer();
                    if (isOk()) await TaskHelpers.TryDelay(checkWaitTime, stop);
                    DeviceMonitorManager.OpenComputer();
                    return;
                }
                catch (TaskCanceledException e)
                {
                    Logger.Debug(MinerTag(), $"XMRingStartedWaitTime TaskCanceledException: {e.Message}");
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error(MinerTag(), $"Exception {e.Message}");
                }
            }
            finally
            {
                Logger.Info(MinerTag(), $"XMRingStartedWaitTime END");
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
                Logger.Error(MinerTag(), $"Stop: {e.Message}");
            }
        }


        #region MinerApiWatchdog
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
            var perDevSpeedsTotalSum = apiData.AlgorithmSpeedsPerDevice?.Values.SelectMany(pl => pl).Select(p => p.speed).Sum() ?? 0d;
            if (perDevSpeedsTotalSum == 0d)
            {
                // TODO debug log speeds are zero
                return;
            }
            if (perDevSpeedsTotalSum == _lastPerDevSpeedsTotalSum)
            {
                // TODO debug log speeds seem to be stuck
                return;
            }
            // update 
            _lastPerDevSpeedsTotalSum = perDevSpeedsTotalSum;
            MinerApiWatchdog.UpdateApiTimestamp(GroupKey, DateTime.UtcNow);
        }
        #endregion MinerApiWatchdog
    }
}
