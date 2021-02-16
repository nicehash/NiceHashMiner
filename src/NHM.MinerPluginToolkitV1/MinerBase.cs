using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1
{
    /// <summary>
    /// MinerBase class implements most common IMiner features and supports MinerOptionsPackage, MinerSystemEnvironmentVariables, MinerReservedApiPorts integration, process watchdog functionality.
    /// </summary>
    public abstract class MinerBase : IMiner, IBinAndCwdPathsGettter
    {
        /// <summary>
        /// This is internal ID counter used for logging
        /// </summary>
        private static ulong _MINER_COUNT_ID = 0;
        /// <summary>
        /// This is tag used for logging, specific for each <see cref="MinerBase"/> object
        /// </summary>
        protected string _baseTag { get; private set; }
        /// <summary>
        /// LogGroup is used to deferentiate between different miners in logs
        /// </summary>
        protected string _logGroup { get; set; }
        /// <summary>
        /// UUID is universal unique identifier for each <see cref="MinerBase"/> object
        /// </summary>
        protected string _uuid { get; private set; }
        /// <summary>
        /// This holds MinerProcess (<see cref="Process"/>)
        /// </summary>
        protected Process _miningProcess;
        /// <summary>
        /// This is a collection of <see cref="MiningPair"/>s 
        /// </summary>
        protected IEnumerable<MiningPair> _miningPairs;
        protected string _miningLocation;
        protected string _username;
        protected string _password;

        // most miners mine on a single algo and most have extra launch params
        protected AlgorithmType _algorithmType;
        protected string _extraLaunchParameters = "";

        public MinerBase(string uuid)
        {
            _MINER_COUNT_ID++;
            _uuid = uuid;
            _baseTag = $"Miner{_MINER_COUNT_ID}-{uuid}";
            _logGroup = $"Miner{_MINER_COUNT_ID}-{uuid}";
        }

        protected object _lock = new object();

        private class StopMinerWatchdogException : Exception
        { }

        public MinerOptionsPackage MinerOptionsPackage { get; set; }
        public MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables { get; set; }

        public MinerReservedPorts MinerReservedApiPorts { get; set; }

        public MinerBenchmarkTimeSettings MinerBenchmarkTimeSettings { get; set; }
        public MinerCustomActionSettings MinerCustomActionSettings { get; set; }

        public IPluginSupportedAlgorithmsSettings PluginSupportedAlgorithms { get; set; }

        abstract public Task<ApiData> GetMinerStatsDataAsync();

        abstract protected void Init();

        // override this on plugins with mapped ids or special mining pairs sorting
        protected virtual IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // by default sort by base id (Cuda and OpenCL ids) and type
            pairsList.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID) - a.Device.DeviceType.CompareTo(b.Device.DeviceType));
            return pairsList;
        }

        public virtual void InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            // now should be ordered
            _miningPairs = GetSortedMiningPairs(miningPairs);
            // update log group
            try
            {
                var devs = _miningPairs.Select(pair => $"{pair.Device.DeviceType}:{pair.Device.ID}");
                var devsTag = $"devs({string.Join(",", devs)})";
                var algo = _miningPairs.First().Algorithm.AlgorithmName;
                var algoTag = $"algo({algo})";
                _logGroup = $"{_baseTag}-{algoTag}-{devsTag}";
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error while setting _logGroup: {e.Message}");
            }

            // init algo, ELP and finally miner specific init
            // init algo
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Info(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }
            // init ELP, _miningPairs are ordered and ELP parsing keeps ordering
            if (MinerOptionsPackage != null)
            {
                var miningPairsList = _miningPairs.ToList();
                var ignoreDefaults = MinerOptionsPackage.IgnoreDefaultValueOptions;
                var generalParams = ExtraLaunchParametersParser.Parse(miningPairsList, MinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var temperatureParams = ExtraLaunchParametersParser.Parse(miningPairsList, MinerOptionsPackage.TemperatureOptions, ignoreDefaults);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
            // miner specific init
            Init();
        }

        public void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x")
        {
            _miningLocation = miningLocation;
            _username = username;
            _password = password;
        }

        // Parent plugin has this info
        public IBinAndCwdPathsGettter BinAndCwdPathsGettter { get; set; } = null;

        public (string binPath, string cwdPath) GetBinAndCwdPaths()
        {
            return BinAndCwdPathsGettter.GetBinAndCwdPaths();
        }

        abstract protected string MiningCreateCommandLine();

        protected virtual Dictionary<string, string> GetEnvironmentVariables()
        {
            if (MinerSystemEnvironmentVariables != null)
            {
                var customSettingKey = MinerToolkit.GetAlgorithmCustomSettingKey(_miningPairs);
                if (MinerSystemEnvironmentVariables.CustomSystemEnvironmentVariables != null && MinerSystemEnvironmentVariables.CustomSystemEnvironmentVariables.ContainsKey(customSettingKey))
                {
                    return MinerSystemEnvironmentVariables.CustomSystemEnvironmentVariables[customSettingKey];
                }

                return MinerSystemEnvironmentVariables.DefaultSystemEnvironmentVariables;
            }
            return null;
        }

        /// <summary>
        /// Provides available port for miner API binding
        /// </summary>
        public virtual int GetAvaliablePort()
        {
            Dictionary<string, List<int>> reservedPorts = null;
            if (MinerReservedApiPorts != null && MinerReservedApiPorts.UseUserSettings)
            {
                reservedPorts = MinerReservedApiPorts.AlgorithmReservedPorts;
            }
            var reservedPortsKey = MinerToolkit.GetAlgorithmPortsKey(_miningPairs);
            if (reservedPorts != null && reservedPorts.ContainsKey(reservedPortsKey) && reservedPorts[reservedPortsKey] != null)
            {
                var reservedPortsRange = reservedPorts[reservedPortsKey];
                var port = FreePortsCheckerManager.GetAvaliablePortInRange(reservedPortsRange); // retrive custom user port
                if (port > -1) return port;
            }
            // if no custom port return a port in the default range
            return FreePortsCheckerManager.GetAvaliablePortFromSettings(); // use the default range
        }

        protected virtual void ExecMinerCustomActionSettings(bool startTrueStopFalse)
        {
            if (MinerCustomActionSettings == null || !MinerCustomActionSettings.UseUserSettings) return;
            if (MinerCustomActionSettings.AlgorithmCustomActions == null) return;

            try
            {
                var actionScriptsKey = MinerToolkit.GetAlgorithmPortsKey(_miningPairs);
                if (MinerCustomActionSettings.AlgorithmCustomActions.ContainsKey(actionScriptsKey) == false) return;
                var actionsEntry = MinerCustomActionSettings.AlgorithmCustomActions[actionScriptsKey];
                if (actionsEntry == null) return;
                var exePath = startTrueStopFalse ? actionsEntry.StartExePath : actionsEntry.StopExePath;
                var exePathWait = startTrueStopFalse ? actionsEntry.StartExePathWaitExec : actionsEntry.StopExePathWaitExec;
                var pcieBusParams = _miningPairs.Select(p => p.Device).Where(d => d is IGpuDevice).Cast<IGpuDevice>().Select(gpu => gpu.PCIeBusID);
                var args = string.Join(",", pcieBusParams);
                if (exePath == null) return;
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = args,
                    //CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Minimized
                    //UseShellExecute = false
                };
                using (var action = Process.Start(startInfo))
                {
                    if (exePathWait)
                    {
                        // blocking
                        action.WaitForExit();
                    }
                }
            }
            catch (Exception e)
            {
                var commandType = startTrueStopFalse ? "START" : "STOP";
                Logger.Error(_logGroup, $"ExecMinerCustomActionSettings {commandType} error: {e.Message}");
            }
        }

        private void ExitMiningProcess(Process miningProcess)
        {
            try
            {
                var pid = miningProcess?.Id ?? -1;
                miningProcess?.CloseMainWindow();
                // 5 seconds wait for shutdown
                var hasExited = !miningProcess?.WaitForExit(5 * 1000) ?? false;
                var stillRunning = pid > -1 ? Process.GetProcessById(pid) != null : false;
                if (!hasExited || stillRunning)
                {
                    miningProcess?.Kill(); // TODO look for another gracefull shutdown
                }
            }
            catch (Exception e)
            {
                Logger.Info(_logGroup, $"Error occured while stopping the process: {e.Message}");
            }
        }

        public Task MinerProcessTask
        {
            get
            {
                lock (_lock)
                {
                    return _miningProcessTask;
                }
            }
            protected set
            {
                lock (_lock)
                {
                    _miningProcessTask = value;
                }
            }
        }
        CancellationTokenSource _stopMinerTaskSource;
        Task _miningProcessTask;

        public virtual Task<object> StartMiningTask(CancellationToken stop)
        {
            //const int ERROR_FILE_NOT_FOUND = 0x2;
            //const int ERROR_ACCESS_DENIED = 0x5;
            // tsc for started process
            var startProcessTaskCompletionSource = new TaskCompletionSource<object>();

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            var commandLine = MiningCreateCommandLine();
            var environmentVariables = GetEnvironmentVariables();
            var stopActionExec = false;
            _miningProcessTask = Task.Run(() =>
            {
                using (_stopMinerTaskSource = new CancellationTokenSource())
                using (var stopMinerTask = CancellationTokenSource.CreateLinkedTokenSource(stop, _stopMinerTaskSource.Token))
                using (var miningProcess = MinerToolkit.CreateMiningProcess(binPath, binCwd, commandLine, environmentVariables))
                using (var quitMiningProcess = stopMinerTask.Token.Register(() => ExitMiningProcess(miningProcess)))
                {
                    lock (_lock)
                    {
                        _miningProcess = miningProcess;
                    }
                    try
                    {
                        // BEFORE
                        ThrowIfIsStop(stopMinerTask.IsCancellationRequested);
                        if (this is IBeforeStartMining bsm)
                        {
                            bsm.BeforeStartMining();
                        }
                        ThrowIfIsStop(stopMinerTask.IsCancellationRequested);

                        // Logging
                        Logger.Info(_logGroup, $"Starting miner binPath='{binPath}'");
                        Logger.Info(_logGroup, $"Starting miner binCwd='{binCwd}'");
                        Logger.Info(_logGroup, $"Starting miner commandLine='{commandLine}'");
                        var environmentVariablesLog = environmentVariables == null ? "<null>" : string.Join(";", environmentVariables.Select(x => x.Key + "=" + x.Value));
                        Logger.Info(_logGroup, $"Starting miner environmentVariables='{environmentVariablesLog}'");
                        ThrowIfIsStop(stopMinerTask.IsCancellationRequested);

                        // exec START custom scripts if any, must be here if blocking
                        ExecMinerCustomActionSettings(true);

                        if (!miningProcess.Start())
                        {
                            Logger.Info(_logGroup, $"Error occured while starting a new process: {miningProcess.ToString()}");
                            startProcessTaskCompletionSource.SetResult(new InvalidOperationException("Could not start process: " + miningProcess));
                            return;
                        }

                        startProcessTaskCompletionSource.SetResult(true);

                        ThrowIfIsStop(stopMinerTask.IsCancellationRequested);
                        if (this is IAfterStartMining asm)
                        {
                            asm.AfterStartMining();
                        }
                        ThrowIfIsStop(stopMinerTask.IsCancellationRequested);
                        // block while process is running
                        miningProcess.WaitForExit();
                        // exec STOP custom scripts if any, must be here if blocking
                        stopActionExec = true;
                        ExecMinerCustomActionSettings(false);
                    }
                    //catch (Win32Exception ex2)
                    //{
                    //    ////if (ex2.NativeErrorCode == ERROR_FILE_NOT_FOUND) throw ex2;
                    //    //Logger.Info(_logGroup, $"Win32Exception Error occured in StartMining : {ex2.ToString()}");
                    //    //Task.Delay(MinerToolkit.MinerRestartDelayMS, _stopMiner.Token).Wait();
                    //}
                    catch (StopMinerWatchdogException)
                    {
                        Logger.Info(_logGroup, $"Watchdog stopped in StartMining");
                        startProcessTaskCompletionSource.SetResult(false);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(_logGroup, $"Error occured in StartMining : {e.Message}");
                        startProcessTaskCompletionSource.SetResult(e);
                    }
                    finally
                    {
                        if (!stopActionExec)
                        {
                            // exec STOP custom scripts if any, last resort it will not block but it is a fallback option
                            ExecMinerCustomActionSettings(false);
                        }
                    }
                }
            });

            return startProcessTaskCompletionSource.Task;
        }

        public virtual async Task StopMiningTask()
        {
            try
            {
                var waitTask = MinerProcessTask;
                _stopMinerTaskSource?.Cancel();
                if (waitTask != null) await waitTask;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while StopMiningTask: {e.Message}");
            }
        }

        /// <summary>
        /// Checks if <see cref="DesiredRunningState"/> is Stop and if so throw <see cref="StopMinerWatchdogException"/>, also throw that exception if param "<paramref name="isTokenCanceled"/>" equals true
        /// </summary>
        private void ThrowIfIsStop(bool isTokenCanceled)
        {
            if (isTokenCanceled) throw new StopMinerWatchdogException();
        }

        public virtual async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            using (var tickCancelSource = new CancellationTokenSource())
            {
                // determine benchmark time 
                // settup times
                var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 40, 60, 140 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds
                var maxTicks = MinerBenchmarkTimeSettings.ParseBenchmarkTicks(new List<int> { 1, 3, 9 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType);
                var maxTicksEnabled = MinerBenchmarkTimeSettings.MaxTicksEnabled;

                //// use demo user and disable the watchdog
                var commandLine = MiningCreateCommandLine();
                var (binPath, binCwd) = GetBinAndCwdPaths();
                Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
                Logger.Info(_logGroup, $"Benchmarking settings: time={benchmarkTime} ticks={maxTicks} ticksEnabled={maxTicksEnabled}");
                var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());
                // disable line readings and read speeds from API
                bp.CheckData = null;

                var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
                var benchmarkWait = TimeSpan.FromMilliseconds(500);
                var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop, tickCancelSource.Token);

                var stoppedAfterTicks = false;
                var validTicks = 0;
                var ticks = benchmarkTime / 10; // on each 10 seconds tick
                var result = new BenchmarkResult();
                var benchmarkApiData = new List<ApiData>();
                for (var tick = 0; tick < ticks; tick++)
                {
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;
                    await Task.Delay(10 * 1000, stop); // 10 seconds delay
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;

                    var ad = await GetMinerStatsDataAsync();
                    var adTotal = ad.AlgorithmSpeedsTotal();
                    var isTickValid = adTotal.Count > 0 && adTotal.All(pair => pair.speed > 0);
                    benchmarkApiData.Add(ad);
                    if (isTickValid) ++validTicks;
                    if (maxTicksEnabled && validTicks >= maxTicks)
                    {
                        stoppedAfterTicks = true;
                        break;
                    }
                }
                // await benchmark task
                if (stoppedAfterTicks)
                {
                    try
                    {
                        tickCancelSource.Cancel();
                    }
                    catch
                    { }
                }
                await t;
                if (stop.IsCancellationRequested)
                {
                    return t.Result;
                }

                // calc speeds
                // TODO calc std deviaton to reduce invalid benches
                try
                {
                    var nonZeroSpeeds = benchmarkApiData.Where(ad => ad.AlgorithmSpeedsTotal().Count > 0 && ad.AlgorithmSpeedsTotal().All(pair => pair.speed > 0))
                                                        .Select(ad => (ad, ad.AlgorithmSpeedsTotal().Count)).ToList();
                    var speedsFromTotals = new List<(AlgorithmType type, double speed)>();
                    if (nonZeroSpeeds.Count > 0)
                    {
                        var maxAlgoPiarsCount = nonZeroSpeeds.Select(adCount => adCount.Count).Max();
                        var sameCountApiDatas = nonZeroSpeeds.Where(adCount => adCount.Count == maxAlgoPiarsCount).Select(adCount => adCount.ad).ToList();
                        var firstPair = sameCountApiDatas.FirstOrDefault();
                        var speedSums = firstPair.AlgorithmSpeedsTotal().Select(pair => new KeyValuePair<AlgorithmType, double>(pair.type, 0.0)).ToDictionary(x => x.Key, x => x.Value);
                        // sum 
                        foreach (var ad in sameCountApiDatas)
                        {
                            foreach (var pair in ad.AlgorithmSpeedsTotal())
                            {
                                speedSums[pair.type] += pair.speed;
                            }
                        }
                        // average
                        foreach (var algoId in speedSums.Keys.ToArray())
                        {
                            speedSums[algoId] /= sameCountApiDatas.Count;
                        }
                        result = new BenchmarkResult
                        {
                            AlgorithmTypeSpeeds = firstPair.AlgorithmSpeedsTotal().Select(pair => (pair.type, speedSums[pair.type])).ToList(),
                            Success = true
                        };
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(_logGroup, $"benchmarking AlgorithmSpeedsTotal error {e.Message}");
                }

                // return API result
                return result;
            }

        }


    }
}
