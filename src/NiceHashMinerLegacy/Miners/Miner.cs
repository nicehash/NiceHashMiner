using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common.Enums;
using Timer = System.Timers.Timer;
using MinerPlugin;

namespace NiceHashMiner
{
    // 
    public class MinerPidData
    {
        public string MinerBinPath;
        public int Pid = -1;
    }

    public abstract class Miner
    {
        // MinerIDCount used to identify miners creation
        protected static long MinerIDCount { get; private set; }


        public NhmConectionType ConectionType { get; protected set; }

        // used to identify miner instance
        protected readonly long MinerID;

        private string _minerTag;
        public string MinerDeviceName { get; set; }

        protected int ApiPort { get; private set; }

        // if miner has no API bind port for reading curentlly only CryptoNight on ccminer
        public bool IsApiReadException { get; protected set; }

        public bool IsNeverHideMiningWindow { get; protected set; }

        // mining algorithm stuff
        protected bool IsInit { get; private set; }

        public MiningSetup MiningSetup { get; protected set; }

        public bool IsRunning { get; protected set; }
        protected string Path { get; private set; }

        protected string LastCommandLine { get; set; }

        // the defaults will be 
        protected string WorkingDirectory { get; private set; }

        protected NiceHashProcess ProcessHandle;
        protected MinerPidData _currentPidData;
        protected readonly List<MinerPidData> _allPidData = new List<MinerPidData>();

        // Benchmark stuff
        protected Exception BenchmarkException;
        protected List<string> BenchLines;


        // TODO maybe set for individual miner cooldown/retries logic variables
        // this replaces MinerAPIGraceSeconds(AMD)
        private const int MinCooldownTimeInMilliseconds = 5 * 1000; // 5 seconds
        //private const int _MIN_CooldownTimeInMilliseconds = 1000; // TESTING

        //private const int _MAX_CooldownTimeInMilliseconds = 60 * 1000; // 1 minute max, whole waiting time 75seconds
        private readonly int _maxCooldownTimeInMilliseconds; // = GetMaxCooldownTimeInMilliseconds();

        protected abstract int GetMaxCooldownTimeInMilliseconds();
        private Timer _cooldownCheckTimer;
        protected MinerApiReadStatus CurrentMinerReadStatus { get; set; }
        private int _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
        private int _currentCooldownTimeInSecondsLeft = MinCooldownTimeInMilliseconds;
        private const int IsCooldownCheckTimerAliveCap = 15;
        private bool _needsRestart;

        public bool _isEnded { get; private set; }

        public bool IsUpdatingApi { get; protected set; } = false;

        // for ApiData and ID of plugins
        public string MinerUUID { get; protected set; }

//// PRODUCTION
//#if !(TESTNET || TESTNETDEV)
        protected Dictionary<string, string> _enviormentVariables = null;
//#endif
//// TESTNET
//#if TESTNET || TESTNETDEV
        protected IEnumerable<ComputeDevice> Devices => MiningSetup.MiningPairs.Select(p => p.Device);
//#endif


        protected Miner(string minerDeviceName)
        {
            ConectionType = NhmConectionType.STRATUM_TCP;
            MiningSetup = new MiningSetup(null);
            IsInit = false;
            MinerID = MinerIDCount++;

            MinerDeviceName = minerDeviceName;

            WorkingDirectory = "";

            IsRunning = false;

            LastCommandLine = "";

            IsApiReadException = false;
            // Only set minimize if hide is false (specific miners will override true after)
            IsNeverHideMiningWindow = ConfigManager.GeneralConfig.MinimizeMiningWindows &&
                                      !ConfigManager.GeneralConfig.HideMiningWindows;

            _maxCooldownTimeInMilliseconds = GetMaxCooldownTimeInMilliseconds();
            // 
            Helpers.ConsolePrint(MinerTag(), "NEW MINER CREATED");
        }

        private void SetApiPort()
        {
            if (IsInit)
            {
                ApiPort = -1; // not set
                ApiPort = MinerPluginToolkitV1.MinersApiPortsManager.GetAvaliablePortInRange(ConfigManager.GeneralConfig.ApiBindPortPoolStart);
            }
        }

        public virtual void InitMiningSetup(MiningSetup miningSetup)
        {
            MiningSetup = miningSetup;
            IsInit = MiningSetup.IsInit;
            SetApiPort();
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
                var ids = MiningSetup.MiningPairs.Select(cdevs => cdevs.Device.ID.ToString()).ToList();
                _minerTag = string.Format(mask, MinerDeviceName, MinerID, string.Join(",", ids));
            }

            return _minerTag;
        }

        private static string ProcessTag(MinerPidData pidData)
        {
            return $"[pid({pidData.Pid})|bin({pidData.MinerBinPath})]";
        }

        public string ProcessTag()
        {
            return _currentPidData == null ? "PidData is NULL" : ProcessTag(_currentPidData);
        }

        public void KillAllUsedMinerProcesses()
        {
            var toRemovePidData = new List<MinerPidData>();
            Helpers.ConsolePrint(MinerTag(), "Trying to kill all miner processes for this instance:");
            foreach (var pidData in _allPidData)
            {
                try
                {
                    var process = Process.GetProcessById(pidData.Pid);
                    if (pidData.MinerBinPath.Contains(process.ProcessName))
                    {
                        Helpers.ConsolePrint(MinerTag(), $"Trying to kill {ProcessTag(pidData)}");
                        try
                        {
                            process.Kill();
                            process.Close();
                            process.WaitForExit(1000 * 60 * 1);
                        }
                        catch (Exception e)
                        {
                            Helpers.ConsolePrint(MinerTag(),
                                $"Exception killing {ProcessTag(pidData)}, exMsg {e.Message}");
                        }
                    }
                }
                catch (Exception e)
                {
                    toRemovePidData.Add(pidData);
                    Helpers.ConsolePrint(MinerTag(), $"Nothing to kill {ProcessTag(pidData)}, exMsg {e.Message}");
                }
            }

            _allPidData.RemoveAll(x => toRemovePidData.Contains(x));
        }

        public abstract void Start(string miningLocation, string username);


        protected abstract void _Stop(MinerStopType willswitch);

        public virtual void Stop(MinerStopType willswitch = MinerStopType.SWITCH)
        {
            _cooldownCheckTimer?.Stop();
            _Stop(willswitch);
            IsRunning = false;
        }

        public void End()
        {
            _isEnded = true;
            Stop(MinerStopType.FORCE_END);
        }

        #region BENCHMARK DE-COUPLED Decoupled benchmarking routines

        // Put this in internals common error lines when benchmarking
        protected void CheckOutdata(string outdata)
        {
            //Helpers.ConsolePrint("BENCHMARK" + benchmarkLogPath, outdata);
            BenchLines.Add(outdata);
            // ccminer, cpuminer
            if (outdata.Contains("Cuda error"))
                BenchmarkException = new Exception("CUDA error");
            if (outdata.Contains("is not supported"))
                BenchmarkException = new Exception("N/A");
            if (outdata.Contains("illegal memory access"))
                BenchmarkException = new Exception("CUDA error");
            if (outdata.Contains("unknown error"))
                BenchmarkException = new Exception("Unknown error");
            if (outdata.Contains("No servers could be used! Exiting."))
                BenchmarkException = new Exception("No pools or work can be used for benchmarking");
            if (outdata.Contains("Error CL_INVALID_KERNEL"))
                BenchmarkException = new Exception("Error CL_INVALID_KERNEL");
            if (outdata.Contains("Error CL_INVALID_KERNEL_ARGS"))
                BenchmarkException = new Exception("Error CL_INVALID_KERNEL_ARGS");
            //if (outdata.Contains("error") || outdata.Contains("Error"))
            //    BenchmarkException = new Exception("Unknown error #2");
            // Ethminer
            if (outdata.Contains("No GPU device with sufficient memory was found"))
                BenchmarkException = new Exception("[daggerhashimoto] No GPU device with sufficient memory was found.");
            // xmr-stak
            if (outdata.Contains("Press any key to exit"))
                BenchmarkException = new Exception("Xmr-Stak erred, check its logs");

            //// lastly parse data
            //if (BenchmarkParseLine(outdata))
            //{
            //    BenchmarkSignalFinnished = true;
            //}
        }

        #endregion //BENCHMARK DE-COUPLED Decoupled benchmarking routines
        

        protected virtual NiceHashProcess _Start()
        {
            // never start when ended
            if (_isEnded)
            {
                return null;
            }

            if (LastCommandLine.Length == 0) return null;

            EthlargementOld.CheckAndStart(MiningSetup);

            var P = new NiceHashProcess();

            if (WorkingDirectory.Length > 1)
            {
                P.StartInfo.WorkingDirectory = WorkingDirectory;
            }

            if (_enviormentVariables != null)
            {
                foreach (var kvp in _enviormentVariables)
                {
                    P.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                }
            }

            P.StartInfo.FileName = Path;
            P.ExitEvent = Miner_Exited;

            P.StartInfo.Arguments = LastCommandLine;
            if (IsNeverHideMiningWindow)
            {
                P.StartInfo.CreateNoWindow = false;
                if (ConfigManager.GeneralConfig.HideMiningWindows || ConfigManager.GeneralConfig.MinimizeMiningWindows)
                {
                    P.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    P.StartInfo.UseShellExecute = true;
                }
            }
            else
            {
                P.StartInfo.CreateNoWindow = ConfigManager.GeneralConfig.HideMiningWindows;
            }

            P.StartInfo.UseShellExecute = false;

            try
            {
                if (P.Start())
                {
                    IsRunning = true;

                    _currentPidData = new MinerPidData
                    {
                        MinerBinPath = P.StartInfo.FileName,
                        Pid = P.Id
                    };
                    _allPidData.Add(_currentPidData);

                    Helpers.ConsolePrint(MinerTag(), "Starting miner " + ProcessTag() + " " + LastCommandLine);

                    StartCoolDownTimerChecker();

                    return P;
                }

                Helpers.ConsolePrint(MinerTag(), "NOT STARTED " + ProcessTag() + " " + LastCommandLine);
                return null;
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " _Start: " + ex.Message);
                return null;
            }
        }

        protected void StartCoolDownTimerChecker()
        {
            if (ConfigManager.GeneralConfig.CoolDownCheckEnabled)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Starting cooldown checker");
                if (_cooldownCheckTimer != null && _cooldownCheckTimer.Enabled) _cooldownCheckTimer.Stop();
                // cool down init
                _cooldownCheckTimer = new Timer()
                {
                    Interval = MinCooldownTimeInMilliseconds
                };
                _cooldownCheckTimer.Elapsed += MinerCoolingCheck_Tick;
                _cooldownCheckTimer.Start();
                _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
                _currentCooldownTimeInSecondsLeft = _currentCooldownTimeInSeconds;
            }
            else
            {
                Helpers.ConsolePrint(MinerTag(), "Cooldown checker disabled");
            }

            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
        }


        protected virtual void Miner_Exited()
        {
            ScheduleRestart(5000);
        }

        protected void ScheduleRestart(int ms)
        {
            var restartInMs = ConfigManager.GeneralConfig.MinerRestartDelayMS > ms
                ? ConfigManager.GeneralConfig.MinerRestartDelayMS
                : ms;
            Helpers.ConsolePrint(MinerTag(), ProcessTag() + $" Miner_Exited Will restart in {restartInMs} ms");
            if (ConfigManager.GeneralConfig.CoolDownCheckEnabled)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                _needsRestart = true;
                _currentCooldownTimeInSecondsLeft = restartInMs;
            }
            else
            {
                // directly restart since cooldown checker not running
                Thread.Sleep(restartInMs);
                Restart();
            }
        }

        protected void Restart()
        {
            if (_isEnded) return;
            Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Restarting miner..");
            Stop(MinerStopType.END); // stop miner first
            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);
            ProcessHandle = _Start(); // start with old command line
        }

        public abstract Task<ApiData> GetSummaryAsync();

        public abstract Task<MinerPlugin.ApiData> GetApiDataAsync();

        #region Cooldown/retry logic

        /// <summary>
        /// decrement time for half current half time, if less then min ammend
        /// </summary>
        private void CoolDown()
        {
            if (_currentCooldownTimeInSeconds > MinCooldownTimeInMilliseconds)
            {
                _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
                Helpers.ConsolePrint(MinerTag(),
                    $"{ProcessTag()} Reseting cool time = {MinCooldownTimeInMilliseconds} ms");
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            }
        }

        /// <summary>
        /// increment time for half current half time, if more then max set restart
        /// </summary>
        private void CoolUp()
        {
            _currentCooldownTimeInSeconds *= 2;
            Helpers.ConsolePrint(MinerTag(),
                $"{ProcessTag()} Cooling UP, cool time is {_currentCooldownTimeInSeconds} ms");
            if (_currentCooldownTimeInSeconds > _maxCooldownTimeInMilliseconds)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " MAX cool time exceeded. RESTARTING");
                Restart();
            }
        }

        private void MinerCoolingCheck_Tick(object sender, ElapsedEventArgs e)
        {
            if (_isEnded)
            {
                End();
                return;
            }

            _currentCooldownTimeInSecondsLeft -= (int) _cooldownCheckTimer.Interval;
            // if times up
            if (_currentCooldownTimeInSecondsLeft > 0) return;
            if (_needsRestart)
            {
                _needsRestart = false;
                Restart();
            }
            else
                switch (CurrentMinerReadStatus)
                {
                    case MinerApiReadStatus.GOT_READ:
                        CoolDown();
                        break;
                    case MinerApiReadStatus.READ_SPEED_ZERO:
                        Helpers.ConsolePrint(MinerTag(), ProcessTag() + " READ SPEED ZERO, will cool up");
                        CoolUp();
                        break;
                    case MinerApiReadStatus.RESTART:
                        Restart();
                        break;
                    default:
                        CoolUp();
                        break;
                }

            // set new times left from the CoolUp/Down change
            _currentCooldownTimeInSecondsLeft = _currentCooldownTimeInSeconds;
        }

        #endregion //Cooldown/retry logic
    }
}
