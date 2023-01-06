using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining.Grouping;
using NHMCore.Notifications;
using NHMCore.Schedules;
using NHMCore.Switching;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        // assume steam game is not running
        private static bool _isGameRunning;

        private static bool _isPauseMiningWhenGamingEnabled;

        private static string _deviceToPauseUuid;
        public static bool IsMiningEnabled => _miningDevices.Any();

        private static bool _useScheduler;

        private static CancellationToken _stopMiningManager = CancellationToken.None;
        #region State for mining
        private static string _username = DemoUser.BTC;
        private static Dictionary<AlgorithmType, double> _normalizedProfits = null;

        // TODO make sure _miningDevices and _runningMiners are in sync
        private static List<MiningDevice> _miningDevices = new List<MiningDevice>();
        private static List<BenchmarkingDevice> _benchmarkingDevices = new List<BenchmarkingDevice>();
        private static Dictionary<string, Miner> _runningMiners = new Dictionary<string, Miner>();
        #endregion State for mining

        
        private abstract record Command
        {
            public TaskCompletionSource<object> Tsc { get; init; } = new TaskCompletionSource<object>();
        }

        private record MainCommand : Command;

        private record NormalizedProfitsUpdateCommand(Dictionary<AlgorithmType, double> NormalizedProfits) : MainCommand;

        private record UsernameChangedCommand(string Username) : MainCommand;

        private record MiningProfitSettingsChangedCommand : MainCommand;

        private record MinerRestartLoopNotifyCommand : MainCommand;

        private record UseOptimizationProfilesChangedCommand : MainCommand;

        private record DNSQChangedCommand : MainCommand;

        private record SSLMiningChangedCommand : MainCommand;

        private record PauseMiningWhenGamingModeSettingsChangedCommand(bool IsPauseMiningWhenGamingModeSettingEnabled) : MainCommand;

        private record IsSteamGameRunningChangedCommand(bool IsSteamGameRunning) : MainCommand;

        private record GPUToPauseChangedCommand(string GpuUuid) : MainCommand;

        private record UseSchedulerChangedCommand(bool UseScheduler) : MainCommand;

        #region Deferred Device Commands

        private abstract record DeferredDeviceCommand(ComputeDevice Device) : Command;

        // deffered commands
        private record StartDeviceCommand(ComputeDevice Device) : DeferredDeviceCommand(Device);

        private record StopDeviceCommand(ComputeDevice Device) : DeferredDeviceCommand(Device);

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
            var command = new StartDeviceCommand(device);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        public static Task StopDevice(ComputeDevice device)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new StopDeviceCommand(device);
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

        private static Task DNSQChanged()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new DNSQChangedCommand();
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }
        private static Task SSLMiningChanged()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new SSLMiningChangedCommand();
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task UseOptimizationProfilesChanged()
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new UseOptimizationProfilesChangedCommand();
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

        private static Task PauseMiningWhenGamingModeSettingsChanged(bool isPauseMiningWhenGamingModeEnabled)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new PauseMiningWhenGamingModeSettingsChangedCommand(isPauseMiningWhenGamingModeEnabled);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task SelectedGPUSettingsChanged(string selectedGPUUuid)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new GPUToPauseChangedCommand(selectedGPUUuid);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task IsSteamGameRunningStatusChanged(bool isSteamGameRunning)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new IsSteamGameRunningChangedCommand(isSteamGameRunning);
            _commandQueue.Enqueue(command);
            return command.Tsc.Task;
        }

        private static Task UseSchedulerSettingsChanged(bool useScheduler)
        {
            if (RunninLoops == null) return Task.CompletedTask;
            var command = new UseSchedulerChangedCommand(useScheduler);
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

            MiscSettings.Instance.PropertyChanged += MiscSettingsInstance_PropertyChanged;
            MiningProfitSettings.Instance.PropertyChanged += MiningProfitSettingsInstance_PropertyChanged;

            _isPauseMiningWhenGamingEnabled = MiningSettings.Instance.PauseMiningWhenGamingMode;
            _deviceToPauseUuid = MiningSettings.Instance.DeviceToPauseUuid;
            _useScheduler = MiningSettings.Instance.UseScheduler;
            MiningSettings.Instance.PropertyChanged += MiningSettingsInstance_PropertyChanged;
        }

       
        public static void StartLoops(CancellationToken stop, string username)
        {
            _username = username;
            _stopMiningManager = stop;
            RunninLoops = Task.Run(() => StartLoopsTask(stop));
        }

        public static Task StartLoopsTask(CancellationToken stop)
        {
            var loop1 = MiningManagerCommandQueueLoop(stop);
            var loop2 = MiningManagerMainLoop(stop);
            return Task.WhenAll(loop1, loop2);
        }

        private static void MiscSettingsInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _ = e.PropertyName switch
            {
                nameof(MiscSettings.UseOptimizationProfiles) => UseOptimizationProfilesChanged(),
                nameof(MiscSettings.ResolveNiceHashDomainsToIPs) => DNSQChanged(),
                _ => Task.CompletedTask,
            };            
        }

        private static void MiningProfitSettingsInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _ = MiningProfitSettingsChanged();
        }

        private static void MiningSettingsInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _ = e.PropertyName switch
            {
                nameof(MiningSettings.PauseMiningWhenGamingMode) => PauseMiningWhenGamingModeSettingsChanged(MiningSettings.Instance.PauseMiningWhenGamingMode),
                nameof(MiningSettings.DeviceToPauseUuid) => SelectedGPUSettingsChanged(MiningSettings.Instance.DeviceToPauseUuid),
                nameof(MiningSettings.EnableSSLMining) => SSLMiningChanged(),
                nameof(MiningSettings.UseScheduler) => UseSchedulerSettingsChanged(MiningSettings.Instance.UseScheduler),
                _ => Task.CompletedTask,
            };
        }

        private static async Task StopAndRemoveBenchmark(DeferredDeviceCommand c)
        {
            var stopBenchmark = _benchmarkingDevices.FirstOrDefault(benchDevice => c.Device == benchDevice.Device);
            if (stopBenchmark != null)
            {
                await stopBenchmark.StopBenchmark();
                _benchmarkingDevices.Remove(stopBenchmark);
            }
        }

        private static async Task StopAndRemoveMiner(DeferredDeviceCommand c)
        {
            var stopMiningKey = _runningMiners.Keys.ToArray().Where(key => key.Contains(c.Device.Uuid)).FirstOrDefault();
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
                    .GroupBy(ddc => ddc.Device)
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
                    .Select(c => (command: c, anyAlgoToBenchmark: c.Device.AnyEnabledAlgorithmsNeedBenchmarking(), anyAlgoToMine: c.Device.AlgorithmSettings.Any(GroupSetupUtils.IsAlgoMiningCapable)))
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
                foreach (var stop in stopCommands) stop.Device.State = DeviceState.Stopped; // THIS TRIGERS STATE CHANGE TODO change this at the point where we initiate the actual change
                foreach (var stop in startErrorCommands) stop.Device.State = DeviceState.Error; // THIS TRIGERS STATE CHANGE TODO change this at the point where we initiate the actual change

                // start and group devices for mining
                var devicesToMineChange = startMiningCommands
                    .Select(c => _miningDevices.FirstOrDefault(miningDev => miningDev.Device == c.Device))
                    .Any(MiningDevice.ShouldUpdate);
                bool isValidMiningDevice(ComputeDevice dev) => nonMiningCommands.All(c => c.Device != dev);
                var devicesToMine = _miningDevices
                    .Select(md => md.Device)
                    .Concat(startMiningCommands.Select(c => c.Device))
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
                foreach (var startMining in startMiningCommands) startMining.Device.State = DeviceState.Mining; // THIS TRIGERS STATE CHANGE TODO change this at the point where we initiate the actual change

                // start devices to benchmark or update existing benchmarks algorithms
                var devicesToBenchmark = startBenchmarkingCommands.Select(c => c.Device)
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
                foreach (var startMining in startBenchmarkingCommands) startMining.Device.State = DeviceState.Benchmarking;

                foreach (var c in validCommands)
                {
                    c.Tsc.TrySetResult(true);
                    c.Device.IsPendingChange = false;
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
            var lastDeferredCommandTime = DateTime.UtcNow;
            var steamWatcher = new SteamWatcher();
            bool handleDeferredCommands() => (DateTime.UtcNow - lastDeferredCommandTime).TotalSeconds >= 0.5;
            var deferredCommands = new List<DeferredDeviceCommand>();
            try
            {
                AlgorithmSwitchingManager.Instance.SmaCheck += SwichMostProfitableGroupUpMethod;
                AlgorithmSwitchingManager.Instance.ForceUpdate();

                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                bool isActive() => !stop.IsCancellationRequested;
                _isGameRunning = steamWatcher.IsSteamGameRunning();
                steamWatcher.OnSteamGameStartedChanged += async (s, isRunning) => {
                    await IsSteamGameRunningStatusChanged(isRunning);
                };
                Logger.Info(Tag, "Starting MiningManagerCommandQueueLoop");
                while (isActive())
                {
                    if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);
                    CheckIfIsOnSchedule();
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
                        deferredCommand.Device.IsPendingChange = true; // TODO check if we can be without this one
                        deferredCommand.Device.State = DeviceState.Pending;
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
                AlgorithmSwitchingManager.Instance.SmaCheck -= SwichMostProfitableGroupUpMethod;
                AlgorithmSwitchingManager.Instance.Stop();
                foreach (var groupMiner in _runningMiners.Values)
                {
                    await groupMiner.StopTask();
                }
                _runningMiners.Clear();
                _miningDevices.Clear();
                steamWatcher.Dispose();
            }
        }

        private static async Task MiningManagerMainLoop(CancellationToken stop)
        {
            try
            {
                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                bool isActive() => !stop.IsCancellationRequested;

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
                groupMiner.Dispose();
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
        //        await groupMiner.StartMinerTask(stopMiningManager, _username);
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
                _normalizedProfits = normalizedProfitsUpdateCommand.NormalizedProfits;
            }
            else if (command is UsernameChangedCommand usernameChangedCommand)
            {
                _username = usernameChangedCommand.Username;
            }
            else if (command is PauseMiningWhenGamingModeSettingsChangedCommand pauseMiningWhenGamingModeSettingsChangedCommand)
            {
                _isPauseMiningWhenGamingEnabled = pauseMiningWhenGamingModeSettingsChangedCommand.IsPauseMiningWhenGamingModeSettingEnabled;
                if (!_isPauseMiningWhenGamingEnabled)
                {
                    var dev = AvailableDevices.Devices.FirstOrDefault(d => d.IsGaming == true);
                    if (dev != null) dev.IsGaming = false;
                }
            }
            else if (command is IsSteamGameRunningChangedCommand isSteamGameRunningChangedCommand)
            {
                _isGameRunning = isSteamGameRunningChangedCommand.IsSteamGameRunning;
            }
            else if (command is GPUToPauseChangedCommand gpuToPauseChangedCommand)
            {
                _deviceToPauseUuid = gpuToPauseChangedCommand.GpuUuid;
                
                // unpause device if not mining and not selected
                var devToUnpause = AvailableDevices.Devices.FirstOrDefault(d => d.Uuid != _deviceToPauseUuid && d.IsGaming == true);
                if (devToUnpause != null) devToUnpause.IsGaming = false;

                // set new selected gpu to true
                var newSelectedDev = AvailableDevices.GetDeviceWithUuid(_deviceToPauseUuid);
                if(newSelectedDev != null)
                {
                    newSelectedDev.PauseMiningWhenGamingMode = true;
                    ConfigManager.DeviceConfigFileCommit(newSelectedDev);
                }

                // set previous selected gpu to false
                var oldSelectedDev = AvailableDevices.Devices.FirstOrDefault(d => d.Uuid != _deviceToPauseUuid && d.PauseMiningWhenGamingMode);
                if (oldSelectedDev != null)
                {
                    oldSelectedDev.PauseMiningWhenGamingMode = false;
                    ConfigManager.DeviceConfigFileCommit(oldSelectedDev);
                }
            }

            bool isRestartMinersCommand(Command command) => command
                is UsernameChangedCommand
                or UseOptimizationProfilesChangedCommand
                or DNSQChangedCommand
                or SSLMiningChangedCommand;

            // here we do the deciding
            // to mine we need to have the username mining location set and ofc device to mine with
            if (_username == null || _normalizedProfits == null)
            {
                if (_username == null) Logger.Error(Tag, "_username is null");
                if (_normalizedProfits == null) Logger.Error(Tag, "_normalizedProfits is null");
                await PauseAllMiners();
            }
            else if (isRestartMinersCommand(command))
            {
                // RESTART-STOP-START
                // STOP
                foreach (var miner in _runningMiners.Values) await miner.StopTask();
                // START
                foreach (var miner in _runningMiners.Values) await miner.StartMinerTask(_stopMiningManager, _username);
            }
            else if (_miningDevices.Count == 0)
            {
                await StopAllMinersTask();
                ApplicationStateManager.StopMining();
            }
            else if (_isGameRunning && _isPauseMiningWhenGamingEnabled && _deviceToPauseUuid != null)
            {
                AvailableNotifications.CreateGamingStarted();
                var dev = AvailableDevices.Devices.FirstOrDefault(d => d.Uuid == _deviceToPauseUuid);
                dev.IsGaming = true;
#if NHMWS4
                dev.State = DeviceState.Gaming;
#endif
                bool skipProfitsThreshold = CheckIfShouldSkipProfitsThreshold(command);
                await SwichMostProfitableGroupUpMethodTask(_normalizedProfits, skipProfitsThreshold);
            }
            else if(!_isGameRunning && _isPauseMiningWhenGamingEnabled && command is IsSteamGameRunningChangedCommand)
            {
                AvailableNotifications.CreateGamingFinished();
                var dev = AvailableDevices.Devices.FirstOrDefault(d => d.Uuid == _deviceToPauseUuid);
                dev.IsGaming = false;
                bool skipProfitsThreshold = CheckIfShouldSkipProfitsThreshold(command);
                await SwichMostProfitableGroupUpMethodTask(_normalizedProfits, skipProfitsThreshold);
            }
            else
            {
                ApplicationStateManager.StartMining();
                bool skipProfitsThreshold = CheckIfShouldSkipProfitsThreshold(command);
                await SwichMostProfitableGroupUpMethodTask(_normalizedProfits, skipProfitsThreshold);

            }
        }


        private static bool CheckIfShouldSkipProfitsThreshold(MainCommand command)
        {
            return MiningProfitSettings.Instance.MineRegardlessOfProfit ||
                command is MiningProfitSettingsChangedCommand || 
                command is MinerRestartLoopNotifyCommand;
        }

        // full of state
        private static bool CheckIfProfitable(double currentProfit, bool log = true)
        {
            if (MiningProfitSettings.Instance.MineRegardlessOfProfit)
            {
                if (log) Logger.Info(Tag, $"Mine always regardless of profit");
                return true;
            }
            var currentProfitFIAT = BalanceAndExchangeRates.Instance.ConvertFromBtc(currentProfit);
            var minProfit = MiningProfitSettings.Instance.MinimumProfit;
            _isProfitable = currentProfitFIAT >= minProfit;
            if (log)
            {
                Logger.Info(Tag, $"Current global profit = {currentProfitFIAT:F8} {BalanceAndExchangeRates.Instance.SelectedFiatCurrency}/Day");
                if (!_isProfitable)
                {
                    Logger.Info(Tag, $"Current global profit = NOT PROFITABLE, MinProfit: {minProfit:F8} {BalanceAndExchangeRates.Instance.SelectedFiatCurrency}/Day");
                }
                else
                {
                    var profitabilityInfo = minProfit.ToString("F8") + $" {BalanceAndExchangeRates.Instance.SelectedFiatCurrency}/Day";
                    Logger.Info(Tag, $"Current global profit = IS PROFITABLE, MinProfit: {profitabilityInfo}");
                }
            }

            return _isProfitable;
        }

        async private static void CheckIfIsOnSchedule()
        {
            _useScheduler = MiningSettings.Instance.UseScheduler;
            if (!_useScheduler) return;
            if(!SchedulesManager.Instance.Schedules.Any()) return;

            var shouldStart = false;
            var shouldStop = false;
            var time = DateTime.Now;

            foreach (var schedule in SchedulesManager.Instance.Schedules)
            {
                if (Convert.ToDateTime(schedule.From).ToString() == time.ToString() && schedule.DaysFrom[time.DayOfWeek.ToString()])
                    shouldStart = true;
                if (Convert.ToDateTime(schedule.To).ToString() == time.ToString() && schedule.DaysTo[time.DayOfWeek.ToString()])
                    shouldStop = true;
            }

            if (!shouldStart && !shouldStop) return;

            if (shouldStop && _runningMiners.Any())
            {
                var devicesToStop = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking);
                foreach (var dev in devicesToStop) await StopDevice(dev);
                await StopAllMinersTask();
            }
            else if (shouldStart && !_runningMiners.Any())
            {
                var devicesToStart = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Stopped);
                foreach (var dev in devicesToStart) await StartDevice(dev);
            }
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
                var cmd = ELPManager.Instance.FindAppropriateCommandForAlgoContainer(miningPairs);
                var toStart = Miner.CreateMinerForMining(miningPairs, startKey, cmd);
                if (toStart == null)
                {
                    Logger.Error(Tag, $"CreateMinerForMining for key='{startKey}' returned <null>");
                    continue;
                }
                _runningMiners[startKey] = toStart;
                await toStart.StartMinerTask(_stopMiningManager, _username);
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
