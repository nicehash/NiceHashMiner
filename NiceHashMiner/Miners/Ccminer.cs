using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class Ccminer : Miner
    {
        public Ccminer() : base("ccminer_NVIDIA")
        { }

        // cryptonight benchmark exception
        private int _cryptonightTotalCount = 0;

        private double _cryptonightTotal = 0;
        private const int CryptonightTotalDelim = 2;

        private bool _benchmarkException => MiningSetup.MinerPath == MinerPaths.Data.CcminerCryptonight
                                           || MiningSetup.MinerPath == MinerPaths.Data.CcminerKlausT;

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            if (MiningSetup.MinerPath == MinerPaths.Data.CcminerX11Gost)
            {
                return 60 * 1000 * 3; // wait a little longer
            }
            return 60 * 1000; // 1 minute max, whole waiting time 75seconds
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            var username = GetUsername(btcAdress, worker);

            IsApiReadException = MiningSetup.MinerPath == MinerPaths.Data.CcminerCryptonight;

            var algo = "";
            var apiBind = "";
            if (!IsApiReadException)
            {
                algo = "--algo=" + MiningSetup.MinerName;
                apiBind = " --api-bind=" + ApiPort;
            }

            LastCommandLine = $"{algo} --url={url} --userpass={username}:x {apiBind} " +
                              $"--devices {GetDevicesCommandString()} " +
                              $"{ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA)} ";

            ProcessHandle = _Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        // new decoupled benchmarking routines

        #region Decoupled benchmarking routines

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var timeLimit = (_benchmarkException) ? "" : " --time-limit " + time;
            var commandLine = " --algo=" + algorithm.MinerName +
                              " --benchmark" +
                              timeLimit + " " +
                              ExtraLaunchParametersParser.ParseForMiningSetup(
                                  MiningSetup,
                                  DeviceType.NVIDIA) +
                              " --devices ";

            commandLine += GetDevicesCommandString();

            // cryptonight exception helper variables
            _cryptonightTotalCount = BenchmarkTimeInSeconds / CryptonightTotalDelim;
            _cryptonightTotal = 0.0d;

            return commandLine;
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            // cryptonight exception
            if (_benchmarkException)
            {
                var speedLength = (BenchmarkAlgorithm.NiceHashID == AlgorithmType.CryptoNight) ? 6 : 8;
                if (outdata.Contains("Total: "))
                {
                    var st = outdata.IndexOf("Total:") + 7;
                    var len = outdata.Length - speedLength - st;

                    var parse = outdata.Substring(st, len).Trim();
                    double.TryParse(parse, NumberStyles.Any, CultureInfo.InvariantCulture, out var tmp);

                    // save speed
                    var i = outdata.IndexOf("Benchmark:");
                    var k = outdata.IndexOf("/s");
                    var hashspeed = outdata.Substring(i + 11, k - i - 9);
                    var b = hashspeed.IndexOf(" ");
                    if (hashspeed.Contains("kH/s"))
                        tmp *= 1000;
                    else if (hashspeed.Contains("MH/s"))
                        tmp *= 1000000;
                    else if (hashspeed.Contains("GH/s"))
                        tmp *= 1000000000;

                    _cryptonightTotal += tmp;
                    _cryptonightTotalCount--;
                }
                if (_cryptonightTotalCount <= 0)
                {
                    var spd = _cryptonightTotal / ((double) BenchmarkTimeInSeconds / CryptonightTotalDelim);
                    BenchmarkAlgorithm.BenchmarkSpeed = spd;
                    BenchmarkSignalFinnished = true;
                }

                return false;
            }

            var lastSpeed = BenchmarkParseLine_cpu_ccminer_extra(outdata);
            if (lastSpeed > 0.0d)
            {
                BenchmarkAlgorithm.BenchmarkSpeed = lastSpeed;
                return true;
            }

            if (double.TryParse(outdata, out lastSpeed))
            {
                BenchmarkAlgorithm.BenchmarkSpeed = lastSpeed;
                return true;
            }
            return false;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        #endregion // Decoupled benchmarking routines

        public override async Task<ApiData> GetSummaryAsync()
        {
            // CryptoNight does not have api bind port
            if (!IsApiReadException) return await GetSummaryCpuCcminerAsync();
            // check if running
            if (ProcessHandle == null)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Could not read data from CryptoNight Proccess is null");
                return null;
            }
            try
            {
                Process.GetProcessById(ProcessHandle.Id);
            }
            catch (ArgumentException ex)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Could not read data from CryptoNight reason: " + ex.Message);
                return null; // will restart outside
            }
            catch (InvalidOperationException ex)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Could not read data from CryptoNight reason: " + ex.Message);
                return null; // will restart outside
            }

            var totalSpeed = MiningSetup.MiningPairs
                .Select(miningPair =>
                    miningPair.Device.GetAlgorithm(MinerBaseType.ccminer, AlgorithmType.CryptoNight, AlgorithmType.NONE))
                .Where(algo => algo != null).Sum(algo => algo.BenchmarkSpeed);

            var cryptoNightData = new ApiData(MiningSetup.CurrentAlgorithmType)
            {
                Speed = totalSpeed
            };
            CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
            // check if speed zero
            if (cryptoNightData.Speed == 0) CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
            return cryptoNightData;
        }
    }
}
