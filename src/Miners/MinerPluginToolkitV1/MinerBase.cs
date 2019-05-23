using MinerPlugin;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using System.Diagnostics;
using System.ComponentModel;

namespace MinerPluginToolkitV1
{
    // TODO there is no watchdog
    public abstract class MinerBase : IMiner, IBinAndCwdPathsGettter
    {
        private static ulong _MINER_COUNT_ID = 0;
        private string _baseTag { get; set; }
        protected string _logGroup { get; private set; }
        protected string _uuid { get; private set; }
        protected Process _miningProcess;
        protected IEnumerable<MiningPair> _miningPairs;
        protected string _miningLocation;
        protected string _username;
        protected string _password;

        public MinerBase(string uuid)
        {
            _MINER_COUNT_ID++;
            _uuid = uuid;
            _baseTag = $"Miner{_MINER_COUNT_ID}-{uuid}";
            _logGroup = $"Miner{_MINER_COUNT_ID}-{uuid}";
        }

        // if stop is called then consider this miner obsolete
        HashSet<CancellationTokenSource> _stopMiners = new HashSet<CancellationTokenSource>();
        protected object _lock = new object();

        private enum DesiredRunningState
        {
            NotSet,
            Start,
            Stop
        }

        private class StopMinerWatchdogException : Exception
        {}

        private DesiredRunningState _desiredState = DesiredRunningState.NotSet;
        private uint _minerProcessStarted = 0;

        public MinerOptionsPackage MinerOptionsPackage { get; set; }
        public MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables { get; set; }

        public MinerReservedPorts MinerReservedApiPorts { get; set; }

        abstract public Task<ApiData> GetMinerStatsDataAsync();

        abstract protected void Init();

        public void InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            _miningPairs = miningPairs;
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
            Init();
        }

        public void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x")
        {
            _miningLocation = miningLocation;
            _username = username;
            _password = password;
        }

        abstract public Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard);

        abstract public Tuple<string, string> GetBinAndCwdPaths();
        abstract protected string MiningCreateCommandLine();

        // most don't require extra enviorment vars
        protected virtual Dictionary<string, string> GetEnvironmentVariables() => null;


        public int GetAvaliablePort()
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

        // TODO check mining pairs
        public void StartMining()
        {
            lock(_lock)
            {
                if (_desiredState == DesiredRunningState.Start)
                {
                    Logger.Error(_logGroup, $"Trying to start an already started miner");
                    return;
                }
                Logger.Info(_logGroup, $"Starting miner watchdog task");
                _desiredState = DesiredRunningState.Start;
            }

            //const int ERROR_FILE_NOT_FOUND = 0x2;
            //const int ERROR_ACCESS_DENIED = 0x5;
            Task.Run(() =>
            {
                using (var stopMiner = new CancellationTokenSource())
                {
                    lock (_lock)
                    {
                        _stopMiners.Add(stopMiner);
                    }
                    var run = IsStart() && stopMiner.IsCancellationRequested == false;
                    while (run)
                    {
                        var binPathBinCwdPair = GetBinAndCwdPaths();
                        var binPath = binPathBinCwdPair.Item1;
                        var binCwd = binPathBinCwdPair.Item2;
                        var commandLine = MiningCreateCommandLine();
                        var environmentVariables = GetEnvironmentVariables();

                        using (var miningProcess = MinerToolkit.CreateMiningProcess(binPath, binCwd, commandLine, environmentVariables))
                        {
                            lock (_lock)
                            {
                                _miningProcess = miningProcess;
                            }
                            var quitMiningProcess = stopMiner.Token.Register(() =>
                            {
                                try
                                {
                                    miningProcess?.CloseMainWindow();
                                    // 5 seconds wait for shutdown
                                    var hasExited = !miningProcess?.WaitForExit(5 * 1000) ?? false;
                                    if (!hasExited)
                                    {
                                        miningProcess?.Kill(); // TODO look for another gracefull shutdown
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Info(_logGroup, $"Error occured while stopping the process: {e.Message}");
                                }
                            });
                            try
                            {
                                // mandatory 500ms delay
                                Task.Delay(500).Wait();

                                // BEFORE
                                ThrowIfIsStop(stopMiner.IsCancellationRequested);
                                if (this is IBeforeStartMining bsm)
                                {
                                    bsm.BeforeStartMining();
                                }
                                ThrowIfIsStop(stopMiner.IsCancellationRequested);

                                // Logging
                                Logger.Info(_logGroup, $"Starting miner commandLine='{commandLine}'");
                                // TODO this will not print content
                                var environmentVariablesLog = environmentVariables == null ? "<null>" : string.Join(";", environmentVariables.Select(x => x.Key + "=" + x.Value));
                                Logger.Info(_logGroup, $"Starting miner environmentVariables='{environmentVariablesLog}'"); // TODO log or debug???
                                ThrowIfIsStop(stopMiner.IsCancellationRequested);
                                //_stopMiner = new CancellationTokenSource();
                                {
                                    if (!miningProcess.Start())
                                    {
                                        Logger.Info(_logGroup, $"Error occured while starting a new process: {miningProcess.ToString()}");
                                        throw new InvalidOperationException("Could not start process: " + miningProcess);
                                    }

                                    ThrowIfIsStop(stopMiner.IsCancellationRequested);
                                    if (this is IAfterStartMining asm)
                                    {
                                        asm.AfterStartMining();
                                    }
                                    ThrowIfIsStop(stopMiner.IsCancellationRequested);

                                    // block loop until process is running
                                    miningProcess.WaitForExit();
                                }
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
                            }
                            catch (Exception e)
                            {
                                Logger.Info(_logGroup, $"Error occured in StartMining : {e.Message}");
                            }
                            finally
                            {
                                quitMiningProcess.Dispose();
                                // delay restart
                                run = IsStart() && stopMiner.IsCancellationRequested == false;
                                if (run)
                                {
                                    Logger.Error(_logGroup, $"Restart Mining in {MinerToolkit.MinerRestartDelayMS}ms");
                                    Task.Delay(MinerToolkit.MinerRestartDelayMS).Wait();
                                }
                            }
                        }
                    }
                    Logger.Error(_logGroup, $"Exited miner watchdog");
                    lock (_lock)
                    {
                        _stopMiners.Remove(stopMiner);
                    }
                }
            });
        }

        private void ThrowIfIsStop(bool isTokenCanceled)
        {
            var isStopped = false;
            lock (_lock)
            {
                isStopped = _desiredState == DesiredRunningState.Stop;
            }
            if (isStopped || isTokenCanceled) throw new StopMinerWatchdogException();
        }

        private bool IsStart()
        {
            lock (_lock)
            {
                return _desiredState == DesiredRunningState.Start;
            }
        }

        public virtual void StopMining()
        {
            Logger.Info(_logGroup, $"Stop miner called");
            lock (_lock)
            {
                _desiredState = DesiredRunningState.Stop;
                Logger.Info(_logGroup, $"Stop miner called: STOP TASKS IN LIST: {_stopMiners.Count}");
                foreach (var stopMiner in _stopMiners)
                {
                    try
                    {
                        stopMiner?.Cancel();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(_logGroup, $"Error occured while StopMining: {e.Message}");
                    }
                }
            }
        }
    }
}
