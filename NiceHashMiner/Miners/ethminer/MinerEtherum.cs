using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    /// <summary>
    /// For now used only for daggerhashimoto
    /// </summary>
    public abstract class MinerEtherum : Miner
    {
        //ComputeDevice
        protected ComputeDevice DaggerHashimotoGenerateDevice;

        protected readonly string CurrentBlockString;
        private readonly DagGenerationType _dagGenerationType;

        protected bool IsPaused = false;

        protected MinerEtherum(string minerDeviceName, string blockString)
            : base(minerDeviceName)
        {
            CurrentBlockString = blockString;
            _dagGenerationType = ConfigManager.GeneralConfig.EthminerDagGenerationType;
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 90 * 1000; // 1.5 minute max, whole waiting time 75seconds
        }

        protected abstract string GetStartCommandStringPart(string url, string username);
        protected abstract string GetBenchmarkCommandStringPart(Algorithm algorithm);

        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = " ";

            var ids = new List<string>();
            foreach (var mPair in MiningSetup.MiningPairs)
            {
                ids.Add(mPair.Device.ID.ToString());
            }
            deviceStringCommand += string.Join(" ", ids);
            // set dag load mode
            deviceStringCommand += $" --dag-load-mode {GetDagGenerationString(_dagGenerationType)} ";
            if (_dagGenerationType == DagGenerationType.Single
                || _dagGenerationType == DagGenerationType.SingleKeep)
            {
                // set dag generation device
                deviceStringCommand += DaggerHashimotoGenerateDevice.ID.ToString();
            }
            return deviceStringCommand;
        }

        public static string GetDagGenerationString(DagGenerationType type)
        {
            switch (type)
            {
                case DagGenerationType.Parallel:
                    return "parallel";
                case DagGenerationType.Sequential:
                    return "sequential";
                case DagGenerationType.Single:
                    return "single";
                case DagGenerationType.SingleKeep:
                    return "singlekeep";
            }
            return "singlekeep";
        }

        public void Start(string url, string btcAdress, string worker, List<MinerEtherum> usedMiners)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            foreach (var ethminer in usedMiners)
            {
                if (ethminer.MinerID != MinerID && (ethminer.IsRunning || ethminer.IsPaused))
                {
                    Helpers.ConsolePrint(MinerTag(), $"Will end {ethminer.MinerTag()} {ethminer.ProcessTag()}");
                    ethminer.End();
                    System.Threading.Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);
                }
            }

            IsPaused = false;
            if (ProcessHandle == null)
            {
                var username = GetUsername(btcAdress, worker);
                LastCommandLine = GetStartCommandStringPart(url, username) + GetDevicesCommandString();
                ProcessHandle = _Start();
            }
            else
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Resuming ethminer..");
                StartCoolDownTimerChecker();
                StartMining();
            }
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var commandLine = GetBenchmarkCommandStringPart(algorithm) + GetDevicesCommandString();
            Ethereum.GetCurrentBlock(CurrentBlockString);
            commandLine += " --benchmark " + Ethereum.CurrentBlockNum;

            return commandLine;
        }

        public override void InitMiningSetup(MiningSetup miningSetup)
        {
            base.InitMiningSetup(miningSetup);
            // now find the fastest for DAG generation
            var fastestSpeed = double.MinValue;
            foreach (var mPair in MiningSetup.MiningPairs)
            {
                var compareSpeed = mPair.Algorithm.AvaragedSpeed;
                if (fastestSpeed < compareSpeed)
                {
                    DaggerHashimotoGenerateDevice = mPair.Device;
                    fastestSpeed = compareSpeed;
                }
            }
        }

        public override Task<ApiData> GetSummaryAsync()
        {
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType);

            var getSpeedStatus = GetSpeed(out var ismining, out ad.Speed);
            if (GetSpeedStatus.GOT == getSpeedStatus)
            {
                // fix MH/s
                ad.Speed *= 1000 * 1000;
                CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                // check if speed zero
                if (ad.Speed == 0) CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                return Task.FromResult(ad);
            }
            if (GetSpeedStatus.NONE == getSpeedStatus)
            {
                ad.Speed = 0;
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                return Task.FromResult(ad);
            }
            // else if (GetSpeedStatus.EXCEPTION == getSpeedStatus) {
            // we don't restart unles not responding for long time check cooldown logic in Miner
            //Helpers.ConsolePrint(MinerTAG(), "ethminer is not running.. restarting..");
            //IsRunning = false;
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            return Task.FromResult<ApiData>(null);
        }

        protected override NiceHashProcess _Start()
        {
            SetEthminerAPI(ApiPort);
            return base._Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            // prevent logging non runing miner
            if (IsRunning && !IsPaused && willswitch == MinerStopType.SWITCH)
            {
                // daggerhashimoto - we only "pause" mining
                IsPaused = true;
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Pausing ethminer..");
                StopMining();
                return;
            }
            if ((IsRunning || IsPaused) && willswitch != MinerStopType.SWITCH)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Shutting down miner");
            }
            if ((willswitch == MinerStopType.FORCE_END || willswitch == MinerStopType.END) && ProcessHandle != null)
            {
                IsPaused = false; // shutting down means it is not paused
                Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
            }
        }

        // benchmark stuff
        protected override bool BenchmarkParseLine(string outdata)
        {
            if (outdata.Contains("min/mean/max:"))
            {
                var splt = outdata.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                var index = Array.IndexOf(splt, "mean");
                var avg_spd = Convert.ToDouble(splt[index + 2]);
                Helpers.ConsolePrint("BENCHMARK", "Final Speed: " + avg_spd + "H/s");

                BenchmarkAlgorithm.BenchmarkSpeed = avg_spd;
                return true;
            }

            return false;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        #region ethminerAPI

        private enum GetSpeedStatus
        {
            NONE,
            GOT,
            EXCEPTION
        }

        /// <summary>
        /// Initialize ethminer API instance.
        /// </summary>
        /// <param name="port">ethminer's API port.</param>
        private void SetEthminerAPI(int port)
        {
            _mPort = port;
            _mClient = new UdpClient("127.0.0.1", port);
        }

        /// <summary>
        /// Call this to start ethminer. If ethminer is already running, nothing happens.
        /// </summary>
        private void StartMining()
        {
            Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Sending START UDP");
            SendUdp(2);
            IsRunning = true;
        }

        /// <summary>
        /// Call this to stop ethminer. If ethminer is already stopped, nothing happens.
        /// </summary>
        private void StopMining()
        {
            Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Sending STOP UDP");
            SendUdp(1);
            IsRunning = false;
        }

        /// <summary>
        /// Call this to get current ethminer speed. This method may block up to 2 seconds.
        /// </summary>
        /// <param name="ismining">Set to true if ethminer is not mining (has been stopped).</param>
        /// <param name="speed">Current ethminer speed in MH/s.</param>
        /// <returns>False if ethminer is unreachable (crashed or unresponsive and needs restarting).</returns>
        private GetSpeedStatus GetSpeed(out bool ismining, out double speed)
        {
            speed = 0;
            ismining = false;

            SendUdp(3);

            var start = DateTime.Now;

            while ((DateTime.Now - start) < TimeSpan.FromMilliseconds(2000))
            {
                if (_mClient.Available > 0)
                {
                    // read
                    try
                    {
                        var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _mPort);
                        var data = _mClient.Receive(ref ipep);
                        if (data.Length != 8) return GetSpeedStatus.NONE;
                        speed = BitConverter.ToDouble(data, 0);
                        if (speed >= 0) ismining = true;
                        else speed = 0;
                        return GetSpeedStatus.GOT;
                    }
                    catch
                    {
                        Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Could not read data from API bind port");
                        return GetSpeedStatus.EXCEPTION;
                    }
                }
                System.Threading.Thread.Sleep(2);
            }

            return GetSpeedStatus.NONE;
        }

        #region PRIVATE

        private int _mPort;
        private UdpClient _mClient;

        private void SendUdp(int code)
        {
            var data = new byte[1];
            data[0] = (byte) code;
            _mClient.Send(data, data.Length);
        }

        #endregion

        #endregion //ethminerAPI
    }
}
