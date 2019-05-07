using NiceHashMiner.Miners.Grouping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinerPlugin;

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

        public List<Miners.Grouping.MiningPair> MiningPairs { get; protected set; }

        public bool IsRunning { get; protected set; }


        //// TODO maybe set for individual miner cooldown/retries logic variables
        //// this replaces MinerAPIGraceSeconds(AMD)
        //private const int MinCooldownTimeInMilliseconds = 5 * 1000; // 5 seconds
        ////private const int _MIN_CooldownTimeInMilliseconds = 1000; // TESTING

        ////private const int _MAX_CooldownTimeInMilliseconds = 60 * 1000; // 1 minute max, whole waiting time 75seconds
        //private readonly int _maxCooldownTimeInMilliseconds; // = GetMaxCooldownTimeInMilliseconds();

        //protected abstract int GetMaxCooldownTimeInMilliseconds();
        //private Timer _cooldownCheckTimer;
        //protected MinerApiReadStatus CurrentMinerReadStatus { get; set; }
        //private int _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
        //private int _currentCooldownTimeInSecondsLeft = MinCooldownTimeInMilliseconds;
        //private const int IsCooldownCheckTimerAliveCap = 15;
        //private bool _needsRestart;

        public bool _isEnded { get; private set; }

        public bool IsUpdatingApi { get; protected set; } = false;


        protected Miner(string minerDeviceName, List<Miners.Grouping.MiningPair> miningPairs)
        {
            MiningPairs = miningPairs;
            IsInit = MiningPairs != null && MiningPairs.Count > 0;

            MinerDeviceName = minerDeviceName;

            IsRunning = false;
            //_maxCooldownTimeInMilliseconds = GetMaxCooldownTimeInMilliseconds();
            // 
            Helpers.ConsolePrint(MinerTag(), "NEW MINER CREATED");
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
            _isEnded = true;
            Stop();
        }

        #region BENCHMARK DE-COUPLED Decoupled benchmarking routines

        // Put this in internals common error lines when benchmarking
        protected void CheckOutdata(string outdata)
        {
            // Benchmark stuff
            Exception BenchmarkException;
            //Helpers.ConsolePrint("BENCHMARK" + benchmarkLogPath, outdata);
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
        

        //protected virtual NiceHashProcess _Start()
        //{
        //    // never start when ended
        //    if (_isEnded)
        //    {
        //        return null;
        //    }

        //    if (LastCommandLine.Length == 0) return null;

        //    EthlargementOld.CheckAndStart(MiningSetup);

        //    var P = new NiceHashProcess();

        //    if (WorkingDirectory.Length > 1)
        //    {
        //        P.StartInfo.WorkingDirectory = WorkingDirectory;
        //    }

        //    if (_enviormentVariables != null)
        //    {
        //        foreach (var kvp in _enviormentVariables)
        //        {
        //            P.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
        //        }
        //    }

        //    P.StartInfo.FileName = Path;
        //    //P.ExitEvent = Miner_Exited;

        //    P.StartInfo.Arguments = LastCommandLine;
        //    if (IsNeverHideMiningWindow)
        //    {
        //        P.StartInfo.CreateNoWindow = false;
        //        if (ConfigManager.GeneralConfig.HideMiningWindows || ConfigManager.GeneralConfig.MinimizeMiningWindows)
        //        {
        //            P.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        //            P.StartInfo.UseShellExecute = true;
        //        }
        //    }
        //    else
        //    {
        //        P.StartInfo.CreateNoWindow = ConfigManager.GeneralConfig.HideMiningWindows;
        //    }

        //    P.StartInfo.UseShellExecute = false;

        //    try
        //    {
        //        if (P.Start())
        //        {
        //            IsRunning = true;
        //            Helpers.ConsolePrint(MinerTag(), "Starting miner " + ProcessTag() + " " + LastCommandLine);
        //            StartCoolDownTimerChecker();

        //            return P;
        //        }

        //        Helpers.ConsolePrint(MinerTag(), "NOT STARTED " + ProcessTag() + " " + LastCommandLine);
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Helpers.ConsolePrint(MinerTag(), ProcessTag() + " _Start: " + ex.Message);
        //        return null;
        //    }
        //}

        //protected void StartCoolDownTimerChecker()
        //{
        //    if (ConfigManager.GeneralConfig.CoolDownCheckEnabled)
        //    {
        //        Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Starting cooldown checker");
        //        if (_cooldownCheckTimer != null && _cooldownCheckTimer.Enabled) _cooldownCheckTimer.Stop();
        //        // cool down init
        //        _cooldownCheckTimer = new Timer()
        //        {
        //            Interval = MinCooldownTimeInMilliseconds
        //        };
        //        _cooldownCheckTimer.Elapsed += MinerCoolingCheck_Tick;
        //        _cooldownCheckTimer.Start();
        //        _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
        //        _currentCooldownTimeInSecondsLeft = _currentCooldownTimeInSeconds;
        //    }
        //    else
        //    {
        //        Helpers.ConsolePrint(MinerTag(), "Cooldown checker disabled");
        //    }

        //    CurrentMinerReadStatus = MinerApiReadStatus.NONE;
        //}

        

        
        //#region Cooldown/retry logic

        ///// <summary>
        ///// decrement time for half current half time, if less then min ammend
        ///// </summary>
        //private void CoolDown()
        //{
        //    if (_currentCooldownTimeInSeconds > MinCooldownTimeInMilliseconds)
        //    {
        //        _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
        //        Helpers.ConsolePrint(MinerTag(),
        //            $"{ProcessTag()} Reseting cool time = {MinCooldownTimeInMilliseconds} ms");
        //        CurrentMinerReadStatus = MinerApiReadStatus.NONE;
        //    }
        //}

        ///// <summary>
        ///// increment time for half current half time, if more then max set restart
        ///// </summary>
        //private void CoolUp()
        //{
        //    _currentCooldownTimeInSeconds *= 2;
        //    Helpers.ConsolePrint(MinerTag(),
        //        $"{ProcessTag()} Cooling UP, cool time is {_currentCooldownTimeInSeconds} ms");
        //    if (_currentCooldownTimeInSeconds > _maxCooldownTimeInMilliseconds)
        //    {
        //        CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
        //        Helpers.ConsolePrint(MinerTag(), ProcessTag() + " MAX cool time exceeded. RESTARTING");
        //        Restart();
        //    }
        //}

        //private void MinerCoolingCheck_Tick(object sender, ElapsedEventArgs e)
        //{
        //    if (_isEnded)
        //    {
        //        End();
        //        return;
        //    }

        //    _currentCooldownTimeInSecondsLeft -= (int) _cooldownCheckTimer.Interval;
        //    // if times up
        //    if (_currentCooldownTimeInSecondsLeft > 0) return;
        //    if (_needsRestart)
        //    {
        //        _needsRestart = false;
        //        Restart();
        //    }
        //    else
        //        switch (CurrentMinerReadStatus)
        //        {
        //            case MinerApiReadStatus.GOT_READ:
        //                CoolDown();
        //                break;
        //            case MinerApiReadStatus.READ_SPEED_ZERO:
        //                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " READ SPEED ZERO, will cool up");
        //                CoolUp();
        //                break;
        //            case MinerApiReadStatus.RESTART:
        //                Restart();
        //                break;
        //            default:
        //                CoolUp();
        //                break;
        //        }

        //    // set new times left from the CoolUp/Down change
        //    _currentCooldownTimeInSecondsLeft = _currentCooldownTimeInSeconds;
        //}

        //#endregion //Cooldown/retry logic
    }
}
