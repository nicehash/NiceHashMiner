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

        public static bool IsMiningEnabled => _miningDevices.Any();

        private static CancellationToken stopMiningManager = CancellationToken.None;
        #region State for mining
        private static string _username = DemoUser.BTC;
        private static string _miningLocation = null;
        private static Dictionary<AlgorithmType, double> _normalizedProfits = null;

        // TODO make sure _miningDevices and _runningMiners are in sync
        private static List<MiningDevice> _miningDevices = new List<MiningDevice>();
        private static List<BenchmarkingDevice> _benchmarkingDevices = new List<BenchmarkingDevice>();
        private static Dictionary<string, Miner> _runningMiners = new Dictionary<string, Miner>();
        #endregion State for mining

        
        private class Command
        {
            public TaskCompletionSource<object> Tsc { get; private set; } = new TaskCompletionSource<object>();
        }

        private class MainCommand : Command
        {
            //public bool IsMain { get; } = true;
        }

        private class NormalizedProfitsUpdateCommand : MainCommand
        {
            public NormalizedProfitsUpdateCommand(Dictionary<AlgorithmType, double> normalizedProfits) { this.normalizedProfits = normalizedProfits; }
            public Dictionary<AlgorithmType, double> normalizedProfits { get; private set; }
        }

        private class MiningLocationChangedCommand : MainCommand
        {
            public MiningLocationChangedCommand(string miningLocation) { this.miningLocation = miningLocation; }
            public string miningLocation { get; private set; }
        }

        private class UsernameChangedCommand : MainCommand
        {
            public UsernameChangedCommand(string username) { this.username = username; }
            public string username { get; private set; }
        }

        private class MiningProfitSettingsChangedCommand : MainCommand
        { }

        private class MinerRestartLoopNotifyCommand : MainCommand
        { }

        private class RunEthlargementChangedCommand : MainCommand
        { }

        #region Deferred Device Commands

        private class DeferredDeviceCommand : Command
        {
            public ComputeDevice device { get; set; }
        }

        // deffered commands
        private class StartDeviceCommand : DeferredDeviceCommand
        { }

        private class StopDeviceCommand : DeferredDeviceCommand
        { }

        #endregion Deferred Device Commands

        private static readonly TrivialChannel<Command> _commandQueue = new TrivialChannel<Command>();

        public static Task RunninLoops { get; private set; } = null;

        #region Command Tasks

        public static Task ChangeUsername(string username)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new UsernameChangedCommand(username);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        public static Task StartDevice(ComputeDevice device)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new StartDeviceCommand { device = device };
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        public static Task StopDevice(ComputeDevice device)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new StopDeviceCommand { device = device };
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task NormalizedProfitsUpdate(Dictionary<AlgorithmType, double> normalizedProfits)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new NormalizedProfitsUpdateCommand(normalizedProfits);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task MiningLocationChanged(string miningLocation)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new MiningLocationChangedCommand(miningLocation);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task UseEthlargementChanged()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new RunEthlargementChangedCommand();
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }
        private static Task MiningProfitSettingsChanged()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new MiningProfitSettingsChangedCommand();
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        public static Task MinerRestartLoopNotify()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new MinerRestartLoopNotifyCommand();
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static async Task HandleCommand(MainCommand command)
        {
            // check what kind of command is it and ALWAYS set Tsc.Result
            // do stuff with the command
            var commandExecSuccess = true;
            try
            {
                await CheckGroupingAndUpdateMiners(command);
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

            _miningLocation = StratumService.Instance.SelectedOrFallbackServiceLocationCode().miningLocationCode;
            StratumService.Instance.OnServiceLocationChanged += StratumServiceInstance_PropertyChanged;

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

        private static void StratumServiceInstance_PropertyChanged(object sender, string miningLocation)
        {
            _ = MiningLocationChanged(miningLocation);
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

        private static async Task StopAndRemoveBenchmark(DeferredDeviceCommand c)
        {
            var stopBenchmark = _benchmarkingDevices.FirstOrDefault(benchDevice => c.device == benchDevice.Device);
            if (stopBenchmark != null)
            {
                await stopBenchmark.StopBenchmark();
                _benchmarkingDevices.Remove(stopBenchmark);
            }
        }

        private static async Task StopAndRemoveMiner(DeferredDeviceCommand c)
        {
            var stopMiningKey = _runningMiners.Keys.ToArray().Where(key => key.Contains(c.device.Uuid)).FirstOrDefault();
            if (stopMiningKey != null)
            {
                await _runningMiners[stopMiningKey].StopTask();
                _runningMiners.Remove(stopMiningKey);
            }
        }

        private static async Task HandleDeferredCommands(List<DeferredDeviceCommand> deferredCommands)
        {
            try
            {
                var deviceActions = deferredCommands
                    .GroupBy(ddc => ddc.device)
                    .Select(g => g.Select(c => c).ToArray())
                    .Select(commands => (finalCommand: commands.LastOrDefault(), redundantCommands: commands.Reverse().Skip(1)))
                    .ToArray();
                
                var redundantCommands = deviceActions
                    .SelectMany(p => p.redundantCommands)
                    .ToArray();
                // mark redundan't actions as complete
                foreach (var redundantCommand in redundantCommands) redundantCommand.Tsc.TrySetResult(false);

                var validCommands = deviceActions.Select(p => p.finalCommand)
                    .Where(c => c != null)
                    .ToArray();

                var stopCommands = validCommands.Where(c => c is StopDeviceCommand).ToArray();

                var partitionedStartCommands = validCommands
                    .Where(c => c is StartDeviceCommand)
                    .Select(c => (command: c, anyAlgoToBenchmark: c.device.AnyEnabledAlgorithmsNeedBenchmarking(), anyAlgoToMine: c.device.AlgorithmSettings.Any(GroupSetupUtils.IsAlgoMiningCapable)))
                    .ToArray();

                var startBenchmarkingCommands = partitionedStartCommands
                    .Where(t => t.anyAlgoToBenchmark)
                    .Select(t => t.command)
                    .ToArray();

                var startMiningCommands = partitionedStartCommands
                    .Where(t => !t.anyAlgoToBenchmark && t.anyAlgoToMine)
                    .Select(t => t.command)
                    .ToArray();

                var startErrorCommands = partitionedStartCommands
                    .Where(t => !t.anyAlgoToBenchmark && !t.anyAlgoToMine)
                    .Select(t => t.command)
                    .ToArray();

                var nonMiningCommands = stopCommands
                    .Concat(startErrorCommands)
                    .Concat(startBenchmarkingCommands)
                    .ToArray();

                var nonBenchmarkingCommands = stopCommands
                    .Concat(startErrorCommands)
                    .Concat(startMiningCommands)
                    .ToArray();

                // stop all newly obsolete miners 
                foreach (var stopMiner in nonMiningCommands) await StopAndRemoveMiner(stopMiner);
                // stop all newly obsolete benchmarks
                foreach (var stopBenchmark in nonBenchmarkingCommands) await StopAndRemoveBenchmark(stopBenchmark);
                // set the stop and error states
                foreach (var stop in stopCommands) stop.device.State = DeviceState.Stopped; // THIS TRIGERS STATE CHANGE TODO change this at the point where we initiate the actual change
                foreach (var stop in startErrorCommands) stop.device.State = DeviceState.Error; // THIS TRIGERS STATE CHANGE TODO change this at the point where we initiate the actual change

                // start and group devices for mining
                var devicesToMineChange = startMiningCommands
                    .Select(c => _miningDevices.FirstOrDefault(miningDev => miningDev.Device == c.device))
                    .Any(MiningDevice.ShouldUpdate);
                Func<ComputeDevice, bool> isValidMiningDevice = (ComputeDevice dev) => nonMiningCommands.All(c => c.device != dev);
                var devicesToMine = _miningDevices
                    .Select(md => md.Device)
                    .Concat(startMiningCommands.Select(c => c.device))
                    .Distinct()
                    .Where(isValidMiningDevice)
                    .ToArray();
                var anyMiningDeviceInactive = devicesToMine.Any(dev => !_runningMiners.Keys.Any(key => key.Contains(dev.Uuid)));
                var miningDevicesMissmatch = _miningDevices.Count != devicesToMine.Length;
                // check if there is a difference
                if (devicesToMineChange || anyMiningDeviceInactive || miningDevicesMissmatch)
                {
                    _miningDevices = GroupSetupUtils.GetMiningDevices(devicesToMine, true);
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
                    await CheckGroupingAndUpdateMiners(new MainCommand());
                }
                foreach (var startMining in startMiningCommands) startMining.device.State = DeviceState.Mining; // THIS TRIGERS STATE CHANGE TODO change this at the point where we initiate the actual change

                // start devices to benchmark or update existing benchmarks algorithms
                var devicesToBenchmark = startBenchmarkingCommands.Select(c => c.device)
                    .Select(dev => (dev, benchmarkingDev: _benchmarkingDevices.FirstOrDefault(benchDev => benchDev.Device == dev)))
                    .ToArray();
                // to update 
                devicesToBenchmark
                    .Where(p => p.benchmarkingDev != null)
                    .Select(p => p.benchmarkingDev)
                    .ToList()
                    .ForEach(benchDev => benchDev.Update());
                // new benchmarks
                devicesToBenchmark
                    .Where(p => p.benchmarkingDev == null)
                    .Select(p => new BenchmarkingDevice(p.dev))
                    .ToList()
                    .ForEach(benchDev => {
                        benchDev.StartBenchmark();
                        _benchmarkingDevices.Add(benchDev);
                    });
                foreach (var startMining in startBenchmarkingCommands) startMining.device.State = DeviceState.Benchmarking;

                foreach (var c in validCommands)
                {
                    c.Tsc.TrySetResult(true);
                    c.device.IsPendingChange = false;
                }
            }
            catch (Exception e)
            {
                Logger.Info(Tag, $"HandleDeferredCommands error {e.Message}");
            }
            // TODO finally on the deferred commands??
        }

        private static async Task MiningManagerCommandQueueLoop(CancellationToken stop)
        {
            var switchingManager = new AlgorithmSwitchingManager();

            var lastDeferredCommandTime = DateTime.UtcNow;
            Func<bool> handleDeferredCommands = () => (DateTime.UtcNow - lastDeferredCommandTime).TotalSeconds >= 0.5;
            var deferredCommands = new List<DeferredDeviceCommand>();
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
                    if (handleDeferredCommands() && deferredCommands.Any())
                    {
                        await HandleDeferredCommands(deferredCommands);
                        deferredCommands.Clear();
                    }
                    // command handling
                    var (command, hasTimedout, exceptionString) = await _commandQueue.ReadAsync(checkWaitTime, stop);
                    if (command == null)
                    {
                        if (exceptionString != null) Logger.Error(Tag, $"Channel.ReadAsync error: {exceptionString}");
                        continue;
                    }
                    if (command is DeferredDeviceCommand deferredCommand)
                    {
                        deferredCommand.device.IsPendingChange = true; // TODO check if we can be without this one
                        deferredCommand.device.State = DeviceState.Pending;
                        lastDeferredCommandTime = DateTime.UtcNow;
                        deferredCommands.Add(deferredCommand);
                        continue;
                    }
                    if (command is MainCommand mainCommand) await HandleCommand(mainCommand);
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

        private static async Task CheckGroupingAndUpdateMiners(MainCommand command)
        {
            // #1 parse the command
            var commandType = command.GetType().Name;
            Logger.Debug(Tag, $"Command type {commandType}");
            if (command is NormalizedProfitsUpdateCommand normalizedProfitsUpdateCommand)
            {
                _normalizedProfits = normalizedProfitsUpdateCommand.normalizedProfits;
            }
            else if (command is MiningLocationChangedCommand miningLocationChangedCommand)
            {
                var oldLocation = _miningLocation;
                _miningLocation = miningLocationChangedCommand.miningLocation;
                if (_miningLocation == oldLocation) return;
            }
            else if (command is UsernameChangedCommand usernameChangedCommand)
            {
                _username = usernameChangedCommand.username;
            }
            

            // here we do the deciding
            // to mine we need to have the username mining location set and ofc device to mine with
            if (_username == null || _normalizedProfits == null || _miningLocation == null)
            {
                if (_username == null) Logger.Error(Tag, "_username is null");
                if (_normalizedProfits == null) Logger.Error(Tag, "_normalizedProfits is null");
                if (_miningLocation == null) Logger.Error(Tag, "_miningLocation is null");
                await PauseAllMiners();
            }
            else if (command is MiningLocationChangedCommand || command is UsernameChangedCommand || command is RunEthlargementChangedCommand)
            {
                // TODO this is miner restarts on mining location and username change, you should take into account the mining location changed for benchmarking
                // RESTART-STOP-START
                // STOP
                foreach (var miner in _runningMiners.Values) await miner.StopTask();
                // START
                foreach (var miner in _runningMiners.Values) await miner.StartMinerTask(stopMiningManager, _miningLocation, _username);
            }
            else if (_miningDevices.Count == 0)
            {
                await StopAllMinersTask();
                ApplicationStateManager.StopMining();
            }
            else
            {
                ApplicationStateManager.StartMining();
                bool skipProfitsThreshold = command is MiningProfitSettingsChangedCommand || command is MinerRestartLoopNotifyCommand;
                await SwichMostProfitableGroupUpMethodTask(_normalizedProfits, skipProfitsThreshold);
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
                        $"\t\tMOST PROFITABLE ALGO: {device.MostProfitableAlgorithmStringID}, PROFIT: {device.GetCurrentMostProfitValue.ToString(DoubleFormat)}");
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
                await toStart.StartMinerTask(stopMiningManager, _miningLocation, _username);
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
