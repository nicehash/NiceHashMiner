using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    internal class Sgminer : Miner
    {
        private readonly int _gpuPlatformNumber;
        private readonly Stopwatch _benchmarkTimer = new Stopwatch();

        public Sgminer()
            : base("sgminer_AMD")
        {
            _gpuPlatformNumber = ComputeDeviceManager.Available.AmdOpenCLPlatformNum;
            IsKillAllUsedMinerProcs = true;
        }

        // use ONLY for exiting a benchmark
        public void KillSgminer()
        {
            foreach (var process in Process.GetProcessesByName("sgminer"))
            {
                try { process.Kill(); }
                catch (Exception e) { Helpers.ConsolePrint(MinerDeviceName, e.ToString()); }
            }
        }

        public override void EndBenchmarkProcces()
        {
            if (BenchmarkProcessStatus != BenchmarkProcessStatus.Killing && BenchmarkProcessStatus != BenchmarkProcessStatus.DoneKilling)
            {
                BenchmarkProcessStatus = BenchmarkProcessStatus.Killing;
                try
                {
                    Helpers.ConsolePrint("BENCHMARK",
                        $"Trying to kill benchmark process {BenchmarkProcessPath} algorithm {BenchmarkAlgorithm.AlgorithmName}");
                    KillSgminer();
                }
                catch { }
                finally
                {
                    BenchmarkProcessStatus = BenchmarkProcessStatus.DoneKilling;
                    Helpers.ConsolePrint("BENCHMARK",
                        $"Benchmark process {BenchmarkProcessPath} algorithm {BenchmarkAlgorithm.AlgorithmName} KILLED");
                    //BenchmarkHandle = null;
                }
            }
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 90 * 1000; // 1.5 minute max, whole waiting time 75seconds
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            var username = GetUsername(btcAdress, worker);

            LastCommandLine = " --gpu-platform " + _gpuPlatformNumber +
                              " -k " + MiningSetup.MinerName +
                              " --url=" + url +
                              " --userpass=" + username +
                              " -p x " +
                              " --api-listen" +
                              " --api-port=" + ApiPort +
                              " " +
                              ExtraLaunchParametersParser.ParseForMiningSetup(
                                  MiningSetup,
                                  DeviceType.AMD) +
                              " --device ";

            LastCommandLine += GetDevicesCommandString();

            ProcessHandle = _Start();
        }

        // new decoupled benchmarking routines

        #region Decoupled benchmarking routines

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var url = Globals.GetLocationUrl(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
                ConectionType);

            // demo for benchmark
            var username = Globals.DemoUser;

            if (ConfigManager.GeneralConfig.WorkerName.Length > 0)
                username += "." + ConfigManager.GeneralConfig.WorkerName.Trim();

            // cd to the cgminer for the process bins
            var commandLine = " /C \"cd /d " + WorkingDirectory + " && sgminer.exe " +
                              " --gpu-platform " + _gpuPlatformNumber +
                              " -k " + algorithm.MinerName +
                              " --url=" + url +
                              " --userpass=" + username +
                              " -p x " +
                              " --sched-stop " + DateTime.Now.AddSeconds(time).ToString("HH:mm") +
                              " -T --log 10 --log-file dump.txt" +
                              " " +
                              ExtraLaunchParametersParser.ParseForMiningSetup(
                                  MiningSetup,
                                  DeviceType.AMD) +
                              " --device ";

            commandLine += GetDevicesCommandString();

            commandLine += " && del dump.txt\"";

            return commandLine;
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            if (outdata.Contains("Average hashrate:") && outdata.Contains("/s") &&
                BenchmarkAlgorithm.NiceHashID != AlgorithmType.DaggerHashimoto)
            {
                var i = outdata.IndexOf(": ");
                var k = outdata.IndexOf("/s");

                // save speed
                var hashSpeed = outdata.Substring(i + 2, k - i + 2);
                Helpers.ConsolePrint("BENCHMARK", "Final Speed: " + hashSpeed);

                hashSpeed = hashSpeed.Substring(0, hashSpeed.IndexOf(" "));
                var speed = double.Parse(hashSpeed, CultureInfo.InvariantCulture);

                if (outdata.Contains("Kilohash"))
                    speed *= 1000;
                else if (outdata.Contains("Megahash"))
                    speed *= 1000000;

                BenchmarkAlgorithm.BenchmarkSpeed = speed;
                return true;
            }
            if (outdata.Contains($"GPU{MiningSetup.MiningPairs[0].Device.ID}") && outdata.Contains("s):") &&
                BenchmarkAlgorithm.NiceHashID == AlgorithmType.DaggerHashimoto)
            {
                var i = outdata.IndexOf("s):");
                var k = outdata.IndexOf("(avg)");

                // save speed
                var hashSpeed = outdata.Substring(i + 3, k - i + 3).Trim();
                hashSpeed = hashSpeed.Replace("(avg):", "");
                Helpers.ConsolePrint("BENCHMARK", "Final Speed: " + hashSpeed);

                double mult = 1;
                if (hashSpeed.Contains("K"))
                {
                    hashSpeed = hashSpeed.Replace("K", " ");
                    mult = 1000;
                }
                else if (hashSpeed.Contains("M"))
                {
                    hashSpeed = hashSpeed.Replace("M", " ");
                    mult = 1000000;
                }

                hashSpeed = hashSpeed.Substring(0, hashSpeed.IndexOf(" "));
                var speed = double.Parse(hashSpeed, CultureInfo.InvariantCulture) * mult;

                BenchmarkAlgorithm.BenchmarkSpeed = speed;

                return true;
            }
            return false;
        }

        protected override void BenchmarkThreadRoutineStartSettup()
        {
            // sgminer extra settings
            var nhDataIndex = BenchmarkAlgorithm.NiceHashID;

            if (!NHSmaData.HasData)
            {
                Helpers.ConsolePrint("BENCHMARK", "Skipping sgminer benchmark because there is no internet " +
                                                  "connection. Sgminer needs internet connection to do benchmarking.");

                throw new Exception("No internet connection");
            }

            NHSmaData.TryGetPaying(nhDataIndex, out var paying);
            if (paying == 0)
            {
                Helpers.ConsolePrint("BENCHMARK", "Skipping sgminer benchmark because there is no work on Nicehash.com " +
                                                  "[algo: " + BenchmarkAlgorithm.AlgorithmName + "(" + nhDataIndex + ")]");

                throw new Exception("No work can be used for benchmarking");
            }

            _benchmarkTimer.Reset();
            _benchmarkTimer.Start();
            // call base, read only outpus
            //BenchmarkHandle.BeginOutputReadLine();
            base.BenchmarkThreadRoutineStartSettup();
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            if (_benchmarkTimer.Elapsed.TotalSeconds >= BenchmarkTimeInSeconds)
            {
                var resp = GetApiDataAsync(ApiPort, "quit").Result.TrimEnd((char) 0);
                Helpers.ConsolePrint("BENCHMARK", "SGMiner Response: " + resp);
            }
            if (_benchmarkTimer.Elapsed.TotalSeconds >= BenchmarkTimeInSeconds + 2)
            {
                _benchmarkTimer.Stop();
                // this is safe in a benchmark
                KillSgminer();
                BenchmarkSignalHanged = true;
            }
            if (!BenchmarkSignalFinnished && outdata != null)
            {
                CheckOutdata(outdata);
            }
        }

        protected override string GetFinalBenchmarkString()
        {
            if (BenchmarkAlgorithm.BenchmarkSpeed <= 0)
            {
                Helpers.ConsolePrint("sgminer_GetFinalBenchmarkString", International.GetText("sgminer_precise_try"));
                return International.GetText("sgminer_precise_try");
            }
            return base.GetFinalBenchmarkString();
        }

        protected override void BenchmarkThreadRoutine(object commandLine)
        {
            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;
            
            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS * 3); // increase wait for sgminer

            try
            {
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                BenchmarkHandle = BenchmarkStartProcess((string) commandLine);
                BenchmarkThreadRoutineStartSettup();
                // wait a little longer then the benchmark routine if exit false throw
                //var timeoutTime = BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds);
                //var exitSucces = BenchmarkHandle.WaitForExit(timeoutTime * 1000);
                // don't use wait for it breaks everything
                BenchmarkProcessStatus = BenchmarkProcessStatus.Running;
                //while (true)
                //{
                //    var outdata = BenchmarkHandle.StandardOutput.ReadLine();

                //    BenchmarkOutputErrorDataReceivedImpl(outdata);
                //    // terminate process situations
                //    if (BenchmarkSignalQuit
                //        || BenchmarkSignalFinnished
                //        || BenchmarkSignalHanged
                //        || BenchmarkSignalTimedout
                //        || BenchmarkException != null)
                //    {
                //EndBenchmarkProcces();
                // this is safe in a benchmark

                var exited = BenchmarkHandle.WaitForExit((BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds) + 20) * 1000);

                if (!exited) KillSgminer();

                if (BenchmarkSignalTimedout)
                {
                    throw new Exception("Benchmark timedout");
                }
                if (BenchmarkException != null)
                {
                    throw BenchmarkException;
                }
                if (BenchmarkSignalQuit)
                {
                    throw new Exception("Termined by user request");
                }
                if (BenchmarkSignalHanged || !exited)
                {
                    throw new Exception("SGMiner is not responding");
                }
                if (BenchmarkSignalFinnished)
                {
                    //break;
                }
                //    }
                //    else
                //    {
                //        // wait a second reduce CPU load
                //        Thread.Sleep(1000);
                //    }
                //}
            }
            catch (Exception ex)
            {
                BenchmarkThreadRoutineCatch(ex);
            }
            finally
            {
                BenchmarkThreadRoutineFinish();
            }
        }

        #endregion // Decoupled benchmarking routines

        // TODO _currentMinerReadStatus
        public override async Task<ApiData> GetSummaryAsync()
        {
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType);

            var resp = await GetApiDataAsync(ApiPort, "summary");
            if (resp == null)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                return null;
            }
            //// sgminer debug log
            //Helpers.ConsolePrint("sgminer-DEBUG_resp", resp);

            try
            {
                // Checks if all the GPUs are Alive first
                var resp2 = await GetApiDataAsync(ApiPort, "devs");
                if (resp2 == null)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                    return null;
                }
                //// sgminer debug log
                //Helpers.ConsolePrint("sgminer-DEBUG_resp2", resp2);

                var checkGpuStatus = resp2.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 1; i < checkGpuStatus.Length - 1; i++)
                {
                    if (checkGpuStatus[i].Contains("Enabled=Y") && !checkGpuStatus[i].Contains("Status=Alive"))
                    {
                        Helpers.ConsolePrint(MinerTag(),
                            ProcessTag() + " GPU " + i + ": Sick/Dead/NoStart/Initialising/Disabled/Rejecting/Unknown");
                        CurrentMinerReadStatus = MinerApiReadStatus.WAIT;
                        return null;
                    }
                }

                var resps = resp.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                if (resps[1].Contains("SUMMARY"))
                {
                    var data = resps[1].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

                    // Get miner's current total speed
                    var speed = data[4].Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                    // Get miner's current total MH
                    var totalMH = double.Parse(data[18].Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries)[1],
                        new CultureInfo("en-US"));

                    ad.Speed = double.Parse(speed[1]) * 1000;

                    if (totalMH <= PreviousTotalMH)
                    {
                        Helpers.ConsolePrint(MinerTag(), ProcessTag() + " SGMiner might be stuck as no new hashes are being produced");
                        Helpers.ConsolePrint(MinerTag(),
                            ProcessTag() + " Prev Total MH: " + PreviousTotalMH + " .. Current Total MH: " + totalMH);
                        CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                        return null;
                    }

                    PreviousTotalMH = totalMH;
                }
                else
                {
                    ad.Speed = 0;
                }
            }
            catch
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                return null;
            }

            CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
            // check if speed zero
            if (ad.Speed == 0) CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;

            return ad;
        }
    }
}
