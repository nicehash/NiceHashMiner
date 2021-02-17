using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining.Grouping;
using NHMCore.Notifications;
using NHMCore.Switching;
using NHMCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Mining
{
    public static class MiningManager
    {
        private const string Tag = "MiningManager";
        private const string DoubleFormat = "F12";

        // assume profitable
        private static bool _isProfitable = true;
        // assume we have internet
        private static bool _isConnectedToInternet = true;

        public static bool IsMiningEnabled => _miningDevices.Count > 0;

        private static CancellationToken stopMiningManager = CancellationToken.None;
        #region State for mining
        private static string _username = DemoUser.BTC;
        private static string _miningLocation = null;
        private static Dictionary<AlgorithmType, double> _normalizedProfits = null;
        private static IEnumerable<ComputeDevice> _devices = null;

        // TODO make sure _miningDevices and _runningMiners are in sync
        private static List<MiningDevice> _miningDevices = new List<MiningDevice>();
        private static Dictionary<string, Miner> _runningMiners = new Dictionary<string, Miner>();
        #endregion State for mining

        private enum CommandType
        {

            DevicesStartStop,
            StopAllMiners,

            NormalizedProfitsUpdate,
            MiningLocationChanged,
            UsernameChanged,

            // TODO profitability changed
            MiningProfitSettingsChanged,

            // This will be handled like MiningProfitSettingsChanged to skip the profit threshold that can prevent switching
            MinerRestartLoopNotify,

            RunEthlargementChanged
        }
        private class Command
        {
            public Command(CommandType commandType, object commandParameters)
            {
                CommandType = commandType;
                CommandParameters = commandParameters;
            }
            public CommandType CommandType { get; private set; }
            public object CommandParameters { get; private set; }
            public TaskCompletionSource<bool> Tsc { get; private set; } = new TaskCompletionSource<bool>();
        }

        private enum CommandResolutionType
        {
            NONE,
            CheckGroupingAndUpdateMiners,
            RestartCurrentActiveMiners,
            StopAllMiners,
            PauseMining,
            ResumeMining
        }

        private static ConcurrentQueue<Command> _commandQueue { get; set; } = new ConcurrentQueue<Command>();

        public static Task RunninLoops { get; private set; } = null;

        #region Command Tasks
        public static Task StopAllMiners()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.StopAllMiners, null);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        public static Task ChangeUsername(string username)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.UsernameChanged, username);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        public static Task UpdateMiningSession(IEnumerable<ComputeDevice> devices)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.DevicesStartStop, devices);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task NormalizedProfitsUpdate(Dictionary<AlgorithmType, double> normalizedProfits)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.NormalizedProfitsUpdate, normalizedProfits);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task MiningLocationChanged(string miningLocation)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.MiningLocationChanged, miningLocation);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task UseEthlargementChanged()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.RunEthlargementChanged, null);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }
        private static Task MiningProfitSettingsChanged()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.MiningProfitSettingsChanged, null);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        public static Task MinerRestartLoopNotify()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new Command(CommandType.MinerRestartLoopNotify, null);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static async Task HandleCommand(Command command)
        {
            // check what kind of command is it and ALWAYS set Tsc.Result
            // do stuff with the command
            var commandExecSuccess = true;
            try
            {
                var commandResolutionType = CommandResolutionType.NONE;
                Logger.Debug(Tag, $"Command type {command.CommandType}");
                switch (command.CommandType)
                {
                    case CommandType.DevicesStartStop:
                        commandResolutionType = CommandResolutionType.CheckGroupingAndUpdateMiners;
                        if (command.CommandParameters is IEnumerable<ComputeDevice> devices)
                        {
                            _devices = devices;
                            Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        }
                        break;
                    case CommandType.NormalizedProfitsUpdate:
                        commandResolutionType = CommandResolutionType.CheckGroupingAndUpdateMiners;
                        if (command.CommandParameters is Dictionary<AlgorithmType, double> profits)
                        {
                            _normalizedProfits = profits;
                            Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        }
                        break;
                    case CommandType.UsernameChanged:
                        commandResolutionType = CommandResolutionType.RestartCurrentActiveMiners;
                        if (command.CommandParameters is string username)
                        {
                            _username = username;
                            Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        }
                        break;
                    case CommandType.MiningLocationChanged:
                        commandResolutionType = CommandResolutionType.RestartCurrentActiveMiners;
                        var location = command.CommandParameters as string;
                        //if (command.CommandParameters is string location || command.CommandParameters is null)
                        {
                            if (_miningLocation == location)
                            {
                                commandResolutionType = CommandResolutionType.NONE;
                                Logger.Debug(Tag, $"Command type {command.CommandType} Location is same no action needed");
                            }
                            else if (location == null)
                            {
                                commandResolutionType = CommandResolutionType.PauseMining;
                                Logger.Debug(Tag, $"Command type {command.CommandType} Location is null pause mining");
                            }
                            else if (_miningLocation == null)
                            {
                                commandResolutionType = CommandResolutionType.CheckGroupingAndUpdateMiners;
                                Logger.Debug(Tag, $"Command type {command.CommandType} _miningLocation == null CheckGroupingAndUpdateMiners");
                            }

                            _miningLocation = location;
                            Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        }
                        break;
                    case CommandType.StopAllMiners:
                        commandResolutionType = CommandResolutionType.StopAllMiners;
                        Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        break;

                    case CommandType.RunEthlargementChanged:
                        commandResolutionType = CommandResolutionType.RestartCurrentActiveMiners;
                        Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        break;

                    case CommandType.MiningProfitSettingsChanged:
                        commandResolutionType = CommandResolutionType.CheckGroupingAndUpdateMiners;
                        Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        break;
                    case CommandType.MinerRestartLoopNotify:
                        commandResolutionType = CommandResolutionType.CheckGroupingAndUpdateMiners;
                        Logger.Debug(Tag, $"Command type {command.CommandType} Updated");
                        break;

                    default:
                        Logger.Debug(Tag, $"command type not handled {command.CommandType}");
                        break;
                }
                // tasks to await
                Logger.Debug(Tag, $"Command type {command.CommandType} Resolution {commandResolutionType}");
                switch (commandResolutionType)
                {
                    case CommandResolutionType.CheckGroupingAndUpdateMiners:
                        await CheckGroupingAndUpdateMiners(command.CommandType);
                        break;
                    case CommandResolutionType.RestartCurrentActiveMiners:
                        await RestartMiners();
                        break;
                    case CommandResolutionType.StopAllMiners:
                        await StopAllMinersTask();
                        break;
                    case CommandResolutionType.PauseMining:
                        await PauseAllMiners();
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                commandExecSuccess = false;
                Logger.Error(Tag, $"HandleCommand Exception: {e.Message}");
            }
            finally
            {
                // always set command source completed here
                command.Tsc.SetResult(commandExecSuccess);
            }
        }

        #endregion Command Tasks


        static MiningManager()
        {
            ApplicationStateManager.OnInternetCheck += OnInternetCheck;

            _miningLocation = StratumService.Instance.SelectedServiceLocation;

            StratumService.Instance.PropertyChanged += StratumServiceInstance_PropertyChanged;

            MiscSettings.Instance.PropertyChanged += MiscSettingsInstance_PropertyChanged;
            MiningProfitSettings.Instance.PropertyChanged += MiningProfitSettingsInstance_PropertyChanged;
        }

        public static void StartLoops(CancellationToken stop, string username)
        {
            _username = username;
            stopMiningManager = stop;
            RunninLoops = Task.Run(() => StartLoopsTask(stop));
        }

        public static Task StartLoopsTask(CancellationToken stop)
        {
            var loop1 = MiningManagerCommandQueueLoop(stop);
            var loop2 = MiningManagerMainLoop(stop);
            return Task.WhenAll(loop1, loop2);
        }

        private static void StratumServiceInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StratumService.SelectedServiceLocation))
            {
                _ = MiningLocationChanged(StratumService.Instance.SelectedServiceLocation);
            }
            if (e.PropertyName == nameof(StratumService.SelectedFallbackServiceLocation))
            {
                _ = MiningLocationChanged(StratumService.Instance.SelectedFallbackServiceLocation);
            }
        }

        private static void MiscSettingsInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MiscSettings.UseEthlargement))
            {
                _ = UseEthlargementChanged();
            }
        }

        private static void MiningProfitSettingsInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _ = MiningProfitSettingsChanged();
        }

        private static async Task MiningManagerCommandQueueLoop(CancellationToken stop)
        {
            var switchingManager = new AlgorithmSwitchingManager();
            try
            {
                switchingManager.SmaCheck += SwichMostProfitableGroupUpMethod;
                switchingManager.ForceUpdate();

                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                Func<bool> isActive = () => !stop.IsCancellationRequested;
                Logger.Info(Tag, "Starting MiningManagerCommandQueueLoop");
                while (isActive())
                {
                    if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);
                    // command handling
                    if (isActive() && _commandQueue.TryDequeue(out var command))
                    {
                        await HandleCommand(command);
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Logger.Debug(Tag, $"MiningManagerCommandQueueLoop TaskCanceledException: {e.Message}");
            }
            finally
            {
                Logger.Info(Tag, "Exiting MiningManagerCommandQueueLoop run cleanup");
                // cleanup
                switchingManager.SmaCheck -= SwichMostProfitableGroupUpMethod;
                switchingManager.Stop();
                foreach (var groupMiner in _runningMiners.Values)
                {
                    await groupMiner.StopTask();
                }
                _runningMiners.Clear();
                _miningDevices.Clear();
            }
        }

        private static async Task MiningManagerMainLoop(CancellationToken stop)
        {
            try
            {
                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                Func<bool> isActive = () => !stop.IsCancellationRequested;

                // sleep time setting is minimal 1 minute, 19-20s interval
                var preventSleepIntervalElapsedTimeChecker = new ElapsedTimeChecker(TimeSpan.FromSeconds(19), true);
                Logger.Info(Tag, "Starting MiningManagerMainLoop");
                while (isActive())
                {
                    if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);

                    // prevent sleep check
                    if (isActive() && preventSleepIntervalElapsedTimeChecker.CheckAndMarkElapsedTime())
                    {
                        var isMining = IsMiningEnabled;
                        if (isMining)
                        {
                            PInvokeHelpers.PreventSleep();
                        }
                        else
                        {
                            PInvokeHelpers.AllowMonitorPowerdownAndSleep();
                        }
                    }
                    // TODO should we check internet interval here???
                }
            }
            catch (TaskCanceledException e)
            {
                Logger.Info(Tag, $"MiningManagerMainLoop TaskCanceledException: {e.Message}");
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"TaskCanceledException Exception: {e.Message}");
            }
            finally
            {
                Logger.Info(Tag, "Exiting MiningManagerMainLoop run cleanup");
                // cleanup
                PInvokeHelpers.AllowMonitorPowerdownAndSleep();
            }
        }


        private static void OnInternetCheck(object sender, bool isConnectedToInternet)
        {
            _isConnectedToInternet = isConnectedToInternet;
        }

        private static async Task StopAllMinersTask()
        {
            foreach (var groupMiner in _runningMiners.Values)
            {
                await groupMiner.StopTask();
            }
            _runningMiners.Clear();
            _miningDevices.Clear();
        }

        // PauseAllMiners doesn't clear _miningDevices
        private static async Task PauseAllMiners()
        {
            foreach (var groupMiner in _runningMiners.Values)
            {
                await groupMiner.StopTask();
            }
            // TODO the pending state breaks start/stop buttons
            //foreach (var groupMiner in _runningMiners.Values)
            //{
            //    foreach (var pair in groupMiner.MiningPairs)
            //    {
            //        var cDev = AvailableDevices.GetDeviceWithUuid(pair.Device.UUID);
            //        if (cDev == null) continue;
            //        cDev.State = DeviceState.Pending;
            //    }
            //}
            _runningMiners.Clear();
            // TODO set devices to Pending state
        }
        //private static async Task ResumeAllMiners()
        //{
        //    foreach (var groupMiner in _runningMiners.Values)
        //    {
        //        await groupMiner.StartMinerTask(stopMiningManager, _miningLocation, _username);
        //    }
        //    // TODO resume devices to Mining state
        //}

        private static async Task CheckGroupingAndUpdateMiners(CommandType commandType)
        {
            // first update mining devices
            if (commandType == CommandType.DevicesStartStop)
            {
                //// TODO should we check new and old miningDevices???
                //var oldMiningDevices = _miningDevices;
                _miningDevices = GroupSetupUtils.GetMiningDevices(_devices, true);
                if (_miningDevices.Count > 0)
                {
                    GroupSetupUtils.AvarageSpeeds(_miningDevices);
                    //// TODO enable StabilityAnalyzer
                    //// set benchmarked speeds for BenchmarkingAnalyzer
                    //foreach (var miningDev in _miningDevices)
                    //{
                    //    var deviceUuid = miningDev.Device.Uuid;
                    //    foreach (var algorithm in miningDev.Algorithms)
                    //    {
                    //        var speedID = $"{deviceUuid}-{algorithm.AlgorithmStringID}";
                    //        var benchmarkSpeed = new BenchmarkingAnalyzer.BenchmarkSpeed {
                    //            PrimarySpeed = algorithm.BenchmarkSpeed,
                    //            SecondarySpeed = algorithm.SecondaryAveragedSpeed,
                    //        };
                    //        BenchmarkingAnalyzer.SetBenchmarkSpeeds(speedID, benchmarkSpeed);
                    //    }
                    //}
                }
            }
            if (commandType == CommandType.MiningLocationChanged)
            {
                _runningMiners.Clear();
                // re-init mining devices
                _miningDevices = GroupSetupUtils.GetMiningDevices(_devices, true);
                if (_miningDevices.Count > 0)
                {
                    GroupSetupUtils.AvarageSpeeds(_miningDevices);
                }
            }
            // TODO check if there is nothing to check maybe
            if (_normalizedProfits == null)
            {
                Logger.Error(Tag, "Profits is null");
                await PauseAllMiners();
                return;
            }
            if (_miningDevices.Count == 0)
            {
                return;
            }
            if (_miningLocation == null)
            {
                Logger.Error(Tag, "_miningLocation is null");
                await PauseAllMiners();
                return;
            }

            await SwichMostProfitableGroupUpMethodTask(_normalizedProfits, CommandType.MiningProfitSettingsChanged == commandType || CommandType.MinerRestartLoopNotify == commandType);
        }

        private static async Task RestartMiners()
        {
            // STOP
            foreach (var key in _runningMiners.Keys)
            {
                await _runningMiners[key].StopTask();
            }
            // START
            foreach (var key in _runningMiners.Keys)
            {
                await _runningMiners[key].StartMinerTask(stopMiningManager, _miningLocation, _username);
            }
        }


        // full of state
        private static bool CheckIfProfitable(double currentProfit, bool log = true)
        {
            if (MiningProfitSettings.Instance.MineRegardlessOfProfit)
            {
                if (log) Logger.Info(Tag, $"Mine always regardless of profit");
                return true;
            }

            // TODO FOR NOW USD ONLY
            var currentProfitUsd = (currentProfit * BalanceAndExchangeRates.Instance.GetUsdExchangeRate());
            var minProfit = MiningProfitSettings.Instance.MinimumProfit;
            _isProfitable = currentProfitUsd >= minProfit;
            if (log)
            {
                Logger.Info(Tag, $"Current global profit = {currentProfitUsd.ToString("F8")} USD/Day");
                if (!_isProfitable)
                {
                    Logger.Info(Tag, $"Current global profit = NOT PROFITABLE, MinProfit: {minProfit.ToString("F8")} USD/Day");
                }
                else
                {
                    var profitabilityInfo = minProfit.ToString("F8") + " USD/Day";
                    Logger.Info(Tag, $"Current global profit = IS PROFITABLE, MinProfit: {profitabilityInfo}");
                }
            }

            return _isProfitable;
        }

        private static bool CheckIfShouldMine(double currentProfit, bool log = true)
        {
            var isProfitable = CheckIfProfitable(currentProfit, log);

            ApplicationStateManager.SetProfitableState(isProfitable);
            ApplicationStateManager.DisplayNoInternetConnection(!_isConnectedToInternet);

            if (!_isConnectedToInternet && log) Logger.Info(Tag, $"No internet connection! Not able to mine.");

            AvailableNotifications.CreateNotProfitableInfo(isProfitable);

            // if profitable and connected to internet mine
            var shouldMine = isProfitable && _isConnectedToInternet;
            return shouldMine;
        }

        private static void SwichMostProfitableGroupUpMethod(object sender, SmaUpdateEventArgs e)
        {
            _ = NormalizedProfitsUpdate(e.NormalizedProfits);
        }


        private static void CalculateAndUpdateMiningDevicesProfits(Dictionary<AlgorithmType, double> profits)
        {
            foreach (var device in _miningDevices)
            {
                // update/calculate profits
                device.CalculateProfits(profits);
            }
        }

        private static (double currentProfit, double prevStateProfit) GetCurrentAndPreviousProfits()
        {
            var currentProfit = 0.0d;
            var prevStateProfit = 0.0d;
            foreach (var device in _miningDevices)
            {
                // check if device has profitable algo
                if (device.HasProfitableAlgo())
                {
                    currentProfit += device.GetCurrentMostProfitValue;
                    prevStateProfit += device.GetPrevMostProfitValue;
                }
            }
            return (currentProfit, prevStateProfit);
        }

        private static List<AlgorithmContainer> GetMostProfitableAlgorithmContainers()
        {
            var profitableAlgorithmContainers = _miningDevices
                .Where(device => device.HasProfitableAlgo())
                .Select(device => device.GetMostProfitableAlgorithmContainer())
                .ToList();
            return profitableAlgorithmContainers;
        }

        private static async Task SwichMostProfitableGroupUpMethodTask(Dictionary<AlgorithmType, double> normalizedProfits, bool skipProfitsThreshold)
        {
            CalculateAndUpdateMiningDevicesProfits(normalizedProfits);
            var (currentProfit, prevStateProfit) = GetCurrentAndPreviousProfits();

            // log profits scope
            {
                var stringBuilderFull = new StringBuilder();
                stringBuilderFull.AppendLine("Current device profits:");
                foreach (var device in _miningDevices)
                {
                    var stringBuilderDevice = new StringBuilder();
                    stringBuilderDevice.AppendLine($"\tProfits for {device.Device.Uuid} ({device.Device.GetFullName()}):");
                    foreach (var algo in device.Algorithms)
                    {
                        if (algo.IgnoreUntil > DateTime.UtcNow) stringBuilderDevice.Append("\t\t* ");
                        else stringBuilderDevice.Append("\t\t");
                        stringBuilderDevice.AppendLine(
                            $"PROFIT = {algo.CurrentNormalizedProfit.ToString(DoubleFormat)}" +
                            $"\t(SPEED = {algo.AveragedSpeeds[0]:e5}" +
                            $"\t\t| NHSMA = {algo.NormalizedSMAData[0]:e5})" +
                            $"\t[{algo.AlgorithmStringID}]"
                        );
                        if (algo.IsDual)
                        {
                            stringBuilderDevice.AppendLine(
                                $"\t\t\t\t\t  Secondary:\t\t {algo.AveragedSpeeds[1]:e5}" +
                                $"\t\t\t\t  {algo.NormalizedSMAData[1]:e5}"
                            );
                        }
                    }
                    // most profitable
                    stringBuilderDevice.AppendLine(
                        $"\t\tMOST PROFITABLE ALGO: {device.GetMostProfitableString()}, PROFIT: {device.GetCurrentMostProfitValue.ToString(DoubleFormat)}");
                    stringBuilderFull.AppendLine(stringBuilderDevice.ToString());
                }
                Logger.Info(Tag, stringBuilderFull.ToString());
            }

            // check if should mine
            // Only check if profitable inside this method when getting SMA data, cheching during mining is not reliable
            if (CheckIfShouldMine(currentProfit) == false)
            {
                foreach (var device in _miningDevices)
                {
                    device.SetNotMining();
                }
                await PauseAllMiners();
                return;
            }

            // check profit threshold
            Logger.Info(Tag, $"PrevStateProfit {prevStateProfit}, CurrentProfit {currentProfit}");
            if (prevStateProfit > 0 && currentProfit > 0 && !skipProfitsThreshold)
            {
                var a = Math.Max(prevStateProfit, currentProfit);
                var b = Math.Min(prevStateProfit, currentProfit);
                //double percDiff = Math.Abs((PrevStateProfit / CurrentProfit) - 1);
                var percDiff = ((a - b)) / b;
                if (percDiff < SwitchSettings.Instance.SwitchProfitabilityThreshold)
                {
                    // don't switch
                    Logger.Info(Tag, $"Will NOT switch profit diff is {percDiff}, current threshold {SwitchSettings.Instance.SwitchProfitabilityThreshold}");
                    // RESTORE OLD PROFITS STATE
                    foreach (var device in _miningDevices)
                    {
                        device.RestoreOldProfitsState();
                    }
                    return;
                }
                Logger.Info(Tag, $"Will SWITCH profit diff is {percDiff}, current threshold {SwitchSettings.Instance.SwitchProfitabilityThreshold}");
            }
            else if (skipProfitsThreshold)
            {
                Logger.Info(Tag, $"Will SWITCH. MiningProfitSettings have changed");
            }

            // grouping starting and stopping
            // group new miners 
            var newGroupedMiningPairs = GroupingUtils.GetGroupedAlgorithmContainers(GetMostProfitableAlgorithmContainers());
            // check newGroupedMiningPairs and running Groups to figure out what to start/stop and keep running
            var currentRunningGroups = _runningMiners.Keys.ToArray();
            // check which groupMiners should be stopped and which ones should be started and which to keep running
            var noChangeGroupMinersKeys = newGroupedMiningPairs.Where(pair => currentRunningGroups.Contains(pair.Key)).Select(pair => pair.Key).OrderBy(uuid => uuid).ToArray();
            var toStartMinerGroupKeys = newGroupedMiningPairs.Where(pair => !currentRunningGroups.Contains(pair.Key)).Select(pair => pair.Key).OrderBy(uuid => uuid).ToArray();
            var toStopMinerGroupKeys = currentRunningGroups.Where(runningKey => !newGroupedMiningPairs.Keys.Contains(runningKey)).OrderBy(uuid => uuid).ToArray();
            // first stop currently running
            foreach (var stopKey in toStopMinerGroupKeys)
            {
                var stopGroup = _runningMiners[stopKey];
                _runningMiners.Remove(stopKey);
                await stopGroup.StopTask();
            }
            // start new
            var miningLocation = StratumService.Instance.SelectedServiceLocation;
            foreach (var startKey in toStartMinerGroupKeys)
            {
                var miningPairs = newGroupedMiningPairs[startKey];
                var toStart = Miner.CreateMinerForMining(miningPairs, startKey);
                if (toStart == null)
                {
                    Logger.Error(Tag, $"CreateMinerForMining for key='{startKey}' returned <null>");
                    continue;
                }
                _runningMiners[startKey] = toStart;
                await toStart.StartMinerTask(stopMiningManager, miningLocation, _username);
            }
            // log scope
            {
                var stopLog = toStopMinerGroupKeys.Length > 0 ? string.Join(",", toStopMinerGroupKeys) : "EMTPY";
                Logger.Info(Tag, $"Stop Old Mining: ({stopLog})");
                var startLog = toStartMinerGroupKeys.Length > 0 ? string.Join(",", toStartMinerGroupKeys) : "EMTPY";
                Logger.Info(Tag, $"Start New Mining : ({startLog})");
                var sameLog = noChangeGroupMinersKeys.Length > 0 ? string.Join(",", noChangeGroupMinersKeys) : "EMTPY";
                Logger.Info(Tag, $"No change : ({sameLog})");
            }
        }

        // TODO check the stats calculation
        //public static async Task MinerStatsCheck()
        //{
        //    await _semaphore.WaitAsync();
        //    try
        //    {
        //        foreach (var m in _runningMiners.Values)
        //        {
        //            // skip if not running or if await already in progress
        //            if (!m.IsRunning || m.IsUpdatingApi) continue;
        //            var ad = m.GetSummaryAsync();
        //        }
        //        // Update GUI
        //        //ApplicationStateManager.RefreshRates(); // just update the model
        //        // now we shoud have new global/total rate display it
        //        var kwhPriceInBtc = BalanceAndExchangeRates.Instance.GetKwhPriceInBtc();
        //        var profitInBTC = MiningDataStats.GetProfit(kwhPriceInBtc);
        //        ApplicationStateManager.DisplayTotalRate(profitInBTC);
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Error(Tag, $"Error occured while getting mining stats: {e.Message}");
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}
    }
}
