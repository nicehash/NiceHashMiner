using Newtonsoft.Json;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using NiceHashMiner.Configs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace NiceHashMiner.Miners
{
    public class EWBF : Miner
    {

        private class Result
        {
            public uint gpuid { get; set; }
            public uint cudaid { get; set; }
            public string busid { get; set; }
            public uint gpu_status { get; set; }
            public int solver { get; set; }
            public int temperature { get; set; }
            public uint gpu_power_usage { get; set; }
            public uint speed_sps { get; set; }
            public uint accepted_shares { get; set; }
            public uint rejected_shares { get; set; }
        }

        private class JsonApiResponse
        {
            public uint id { get; set; }
            public string method { get; set; }
            public object error { get; set; }
            public List<Result> result { get; set; }
        }

        private int benchmarkTimeWait = 2 * 45;
        private const string LOOK_FOR_START = "total speed: ";
        int benchmark_read_count = 0;
        double benchmark_sum = 0.0d;
        const string LOOK_FOR_END = "sol/s";
        const double DevFee = 2.0;

        public EWBF() : base("ewbf") {
            ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        public override void Start(string url, string btcAdress, string worker) {
            LastCommandLine = GetStartCommand(url, btcAdress, worker);
            var vcp = "msvcp120.dll";
            var vcpPath = WorkingDirectory + vcp;
            if (!File.Exists(vcpPath)) {
                try {
                    File.Copy(vcp, vcpPath, true);
                    Helpers.ConsolePrint(MinerTAG(), String.Format("Copy from {0} to {1} done", vcp, vcpPath));
                } catch (Exception e) {
                    Helpers.ConsolePrint(MinerTAG(), "Copy msvcp.dll failed: " + e.Message);
                }
            }
            ProcessHandle = _Start();
        }

        private string GetStartCommand(string url, string btcAddress, string worker) {
            var ret = GetDevicesCommandString()
                + " --server " + url.Split(':')[0]
                + " --user " + btcAddress + "." + worker + " --pass x --port "
                + url.Split(':')[1] + " --api 127.0.0.1:" + APIPort;
            if (!ret.Contains("--fee")) {
                ret += " --fee 0";
            }
            return ret;
        }

        protected override string GetDevicesCommandString() {
            string deviceStringCommand = " --cuda_devices ";
            foreach (var nvidia_pair in this.MiningSetup.MiningPairs) {
                deviceStringCommand += nvidia_pair.Device.ID + " ";

            }

            deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);

            return deviceStringCommand;
        }

        // benchmark stuff
        protected void KillMinerBase(string exeName) {
            foreach (Process process in Process.GetProcessesByName(exeName)) {
                try { process.Kill(); } catch (Exception e) { Helpers.ConsolePrint(MinerDeviceName, e.ToString()); }
            }
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            CleanAllOldLogs();

            string server = Globals.GetLocationURL(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], this.ConectionType);
            string ret = " --log 2 --logfile benchmark_log.txt" + GetStartCommand(server, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim());
            benchmarkTimeWait = Math.Max(time * 3, 90);  // EWBF takes a long time to get started
            return ret;
        }

        protected override void BenchmarkThreadRoutine(object CommandLine) {
            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);

            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;

            try {
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                Helpers.ConsolePrint(MinerTAG(), "Benchmark should end in : " + benchmarkTimeWait + " seconds");
                BenchmarkHandle = BenchmarkStartProcess((string)CommandLine);
                BenchmarkHandle.WaitForExit(benchmarkTimeWait + 2);
                Stopwatch _benchmarkTimer = new Stopwatch();
                _benchmarkTimer.Reset();
                _benchmarkTimer.Start();
                //BenchmarkThreadRoutineStartSettup();
                // wait a little longer then the benchmark routine if exit false throw
                //var timeoutTime = BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds);
                //var exitSucces = BenchmarkHandle.WaitForExit(timeoutTime * 1000);
                // don't use wait for it breaks everything
                BenchmarkProcessStatus = BenchmarkProcessStatus.Running;
                bool keepRunning = true;
                while (keepRunning && IsActiveProcess(BenchmarkHandle.Id)) {
                    //string outdata = BenchmarkHandle.StandardOutput.ReadLine();
                    //BenchmarkOutputErrorDataReceivedImpl(outdata);
                    // terminate process situations
                    if (_benchmarkTimer.Elapsed.TotalSeconds >= (benchmarkTimeWait + 2)
                        || BenchmarkSignalQuit
                        || BenchmarkSignalFinnished
                        || BenchmarkSignalHanged
                        || BenchmarkSignalTimedout
                        || BenchmarkException != null) {

                        string imageName = MinerExeName.Replace(".exe", "");
                        // maybe will have to KILL process
                        KillMinerBase(imageName);
                        if (BenchmarkSignalTimedout) {
                            throw new Exception("Benchmark timedout");
                        }
                        if (BenchmarkException != null) {
                            throw BenchmarkException;
                        }
                        if (BenchmarkSignalQuit) {
                            throw new Exception("Termined by user request");
                        }
                        if (BenchmarkSignalFinnished) {
                            break;
                        }
                        keepRunning = false;
                        break;
                    } else {
                        // wait a second reduce CPU load
                        Thread.Sleep(1000);
                    }

                }
            } catch (Exception ex) {
                BenchmarkThreadRoutineCatch(ex);
            } finally {
                BenchmarkAlgorithm.BenchmarkSpeed = 0;
                // find latest log file
                string latestLogFile = "";
                var dirInfo = new DirectoryInfo(this.WorkingDirectory);
                foreach (var file in dirInfo.GetFiles("*_log.txt")) {
                    latestLogFile = file.Name;
                    break;
                }
                // read file log
                if (File.Exists(WorkingDirectory + latestLogFile)) {
                    var lines = new string[0];
                    var read = false;
                    var iteration = 0;
                    while (!read) {
                        if (iteration < 10) {
                            try {
                                lines = File.ReadAllLines(WorkingDirectory + latestLogFile);
                                read = true;
                                Helpers.ConsolePrint(MinerTAG(), "Successfully read log after " + iteration.ToString() + " iterations");
                            } catch (Exception ex) {
                                Helpers.ConsolePrint(MinerTAG(), ex.Message);
                                Thread.Sleep(1000);
                            }
                            iteration++;
                        } else {
                            read = true;  // Give up after 10s
                            Helpers.ConsolePrint(MinerTAG(), "Gave up on iteration " + iteration.ToString());
                        }
                    }

                    var addBenchLines = bench_lines.Count == 0;
                    foreach (var line in lines) {
                        if (line != null) {
                            bench_lines.Add(line);
                            string lineLowered = line.ToLower();
                            if (lineLowered.Contains(LOOK_FOR_START)) {
                                benchmark_sum += getNumber(lineLowered);
                                ++benchmark_read_count;
                            }
                        }
                    }
                    if (benchmark_read_count > 0) {
                        BenchmarkAlgorithm.BenchmarkSpeed = benchmark_sum / benchmark_read_count;
                    }
                }
                BenchmarkThreadRoutineFinish();
            }
        }

        protected void CleanAllOldLogs() {
            // clean old logs
            try {
                var dirInfo = new DirectoryInfo(this.WorkingDirectory);
                var deleteContains = "_log.txt";
                if (dirInfo != null && dirInfo.Exists) {
                    foreach (FileInfo file in dirInfo.GetFiles()) {
                        if (file.Name.Contains(deleteContains)) {
                            file.Delete();
                        }
                    }
                }
            } catch { }
        }

        // stub benchmarks read from file
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata) {
            Helpers.ConsolePrint("BENCHMARK", outdata);
            return false;
        }

        protected double getNumber(string outdata) {
            return getNumber(outdata, LOOK_FOR_START, LOOK_FOR_END);
        }

        protected double getNumber(string outdata, string LOOK_FOR_START, string LOOK_FOR_END) {
            try {
                double mult = 1;
                int speedStart = outdata.IndexOf(LOOK_FOR_START);
                string speed = outdata.Substring(speedStart, outdata.Length - speedStart);
                speed = speed.Replace(LOOK_FOR_START, "");
                speed = speed.Substring(0, speed.IndexOf(LOOK_FOR_END));

                if (speed.Contains("k")) {
                    mult = 1000;
                    speed = speed.Replace("k", "");
                } else if (speed.Contains("m")) {
                    mult = 1000000;
                    speed = speed.Replace("m", "");
                }
                //Helpers.ConsolePrint("speed", speed);
                speed = speed.Trim();
                return (Double.Parse(speed, CultureInfo.InvariantCulture) * mult) * (1.0 - DevFee * 0.01);
            } catch (Exception ex) {
                Helpers.ConsolePrint("getNumber", ex.Message + " | args => " + outdata + " | " + LOOK_FOR_END + " | " + LOOK_FOR_START);
            }
            return 0;
        }

        public override APIData GetSummary() {
            _currentMinerReadStatus = MinerAPIReadStatus.NONE;
            APIData ad = new APIData(MiningSetup.CurrentAlgorithmType);

            TcpClient client = null;
            JsonApiResponse resp = null;
            try {
                byte[] bytesToSend = Encoding.ASCII.GetBytes("{\"method\":\"getstat\"}\n");
                client = new TcpClient("127.0.0.1", APIPort);
                NetworkStream nwStream = client.GetStream();
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                string respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                client.Close();
            } catch (Exception ex) {
                Helpers.ConsolePrint(MinerTAG(), ex.Message);
            }

            if (resp != null && resp.error == null) {
                ad.Speed = resp.result.Aggregate<Result, uint>(0, (current, t1) => current + t1.speed_sps);
                _currentMinerReadStatus = MinerAPIReadStatus.GOT_READ;
                if (ad.Speed == 0) {
                    _currentMinerReadStatus = MinerAPIReadStatus.READ_SPEED_ZERO;
                }
            }

            return ad;
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }
    }
}