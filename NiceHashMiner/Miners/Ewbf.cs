using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class Ewbf : Miner
    {
#pragma warning disable IDE1006
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
#pragma warning restore IDE1006

        private int _benchmarkTimeWait = 2 * 45;
        private int _benchmarkReadCount;
        private double _benchmarkSum;
        private const string LookForStart = "total speed: ";
        private const string LookForEnd = "sol/s";
        private const double DevFee = 2.0;

        public Ewbf() : base("ewbf")
        {
            ConectionType = NhmConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = GetStartCommand(url, btcAdress, worker);
            const string vcp = "msvcp120.dll";
            var vcpPath = WorkingDirectory + vcp;
            if (!File.Exists(vcpPath))
            {
                try
                {
                    File.Copy(vcp, vcpPath, true);
                    Helpers.ConsolePrint(MinerTag(), $"Copy from {vcp} to {vcpPath} done");
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint(MinerTag(), "Copy msvcp.dll failed: " + e.Message);
                }
            }

            ProcessHandle = _Start();
        }

        private string GetStartCommand(string url, string btcAddress, string worker)
        {
            var ret = GetDevicesCommandString()
                      + " --server " + url.Split(':')[0]
                      + " --user " + btcAddress + "." + worker + " --pass x --port "
                      + url.Split(':')[1] + " --api 127.0.0.1:" + ApiPort;
            if (!ret.Contains("--fee"))
            {
                ret += " --fee 0";
            }

            return ret;
        }

        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = MiningSetup.MiningPairs.Aggregate(" --cuda_devices ",
                (current, nvidiaPair) => current + (nvidiaPair.Device.ID + " "));

            deviceStringCommand +=
                " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);

            return deviceStringCommand;
        }

        // benchmark stuff
        protected void KillMinerBase(string exeName)
        {
            foreach (var process in Process.GetProcessesByName(exeName))
            {
                try { process.Kill(); }
                catch (Exception e) { Helpers.ConsolePrint(MinerDeviceName, e.ToString()); }
            }
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            CleanOldLogs();

            var server = Globals.GetLocationUrl(algorithm.NiceHashID,
                Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], ConectionType);
            var ret = $" --log 2 --logfile {GetLogFileName()} " + GetStartCommand(server, Globals.GetBitcoinUser(),
                          ConfigManager.GeneralConfig.WorkerName.Trim());
            _benchmarkTimeWait = Math.Max(time * 3, 90); // EWBF takes a long time to get started
            return ret;
        }

        protected override void BenchmarkThreadRoutine(object commandLine)
        {
            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;
            
            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);

            try
            {
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                Helpers.ConsolePrint(MinerTag(), "Benchmark should end in : " + _benchmarkTimeWait + " seconds");
                BenchmarkHandle = BenchmarkStartProcess((string) commandLine);
                BenchmarkHandle.WaitForExit(_benchmarkTimeWait + 2);
                var benchmarkTimer = new Stopwatch();
                benchmarkTimer.Reset();
                benchmarkTimer.Start();
                //BenchmarkThreadRoutineStartSettup();
                // wait a little longer then the benchmark routine if exit false throw
                //var timeoutTime = BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds);
                //var exitSucces = BenchmarkHandle.WaitForExit(timeoutTime * 1000);
                // don't use wait for it breaks everything
                BenchmarkProcessStatus = BenchmarkProcessStatus.Running;
                var keepRunning = true;
                while (keepRunning && IsActiveProcess(BenchmarkHandle.Id))
                {
                    //string outdata = BenchmarkHandle.StandardOutput.ReadLine();
                    //BenchmarkOutputErrorDataReceivedImpl(outdata);
                    // terminate process situations
                    if (benchmarkTimer.Elapsed.TotalSeconds >= (_benchmarkTimeWait + 2)
                        || BenchmarkSignalQuit
                        || BenchmarkSignalFinnished
                        || BenchmarkSignalHanged
                        || BenchmarkSignalTimedout
                        || BenchmarkException != null)
                    {
                        var imageName = MinerExeName.Replace(".exe", "");
                        // maybe will have to KILL process
                        KillMinerBase(imageName);
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

                        if (BenchmarkSignalFinnished)
                        {
                            break;
                        }

                        keepRunning = false;
                        break;
                    }

                    // wait a second reduce CPU load
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                BenchmarkThreadRoutineCatch(ex);
            }
            finally
            {
                BenchmarkAlgorithm.BenchmarkSpeed = 0;
                // find latest log file
                var latestLogFile = "";
                var dirInfo = new DirectoryInfo(WorkingDirectory);
                foreach (var file in dirInfo.GetFiles(GetLogFileName()))
                {
                    latestLogFile = file.Name;
                    break;
                }

                // read file log
                if (File.Exists(WorkingDirectory + latestLogFile))
                {
                    var lines = new string[0];
                    var read = false;
                    var iteration = 0;
                    while (!read)
                    {
                        if (iteration < 10)
                        {
                            try
                            {
                                lines = File.ReadAllLines(WorkingDirectory + latestLogFile);
                                read = true;
                                Helpers.ConsolePrint(MinerTag(),
                                    "Successfully read log after " + iteration + " iterations");
                            }
                            catch (Exception ex)
                            {
                                Helpers.ConsolePrint(MinerTag(), ex.Message);
                                Thread.Sleep(1000);
                            }

                            iteration++;
                        }
                        else
                        {
                            read = true; // Give up after 10s
                            Helpers.ConsolePrint(MinerTag(), "Gave up on iteration " + iteration);
                        }
                    }

                    var addBenchLines = BenchLines.Count == 0;
                    foreach (var line in lines)
                    {
                        if (line != null)
                        {
                            BenchLines.Add(line);
                            var lineLowered = line.ToLower();
                            if (lineLowered.Contains(LookForStart))
                            {
                                _benchmarkSum += GetNumber(lineLowered);
                                ++_benchmarkReadCount;
                            }
                        }
                    }

                    if (_benchmarkReadCount > 0)
                    {
                        BenchmarkAlgorithm.BenchmarkSpeed = _benchmarkSum / _benchmarkReadCount;
                    }
                }

                BenchmarkThreadRoutineFinish();
            }
        }

        // stub benchmarks read from file
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            Helpers.ConsolePrint("BENCHMARK", outdata);
            return false;
        }

        protected double GetNumber(string outdata)
        {
            return GetNumber(outdata, LookForStart, LookForEnd);
        }

        protected double GetNumber(string outdata, string lookForStart, string lookForEnd)
        {
            try
            {
                double mult = 1;
                var speedStart = outdata.IndexOf(lookForStart);
                var speed = outdata.Substring(speedStart, outdata.Length - speedStart);
                speed = speed.Replace(lookForStart, "");
                speed = speed.Substring(0, speed.IndexOf(lookForEnd));

                if (speed.Contains("k"))
                {
                    mult = 1000;
                    speed = speed.Replace("k", "");
                }
                else if (speed.Contains("m"))
                {
                    mult = 1000000;
                    speed = speed.Replace("m", "");
                }

                //Helpers.ConsolePrint("speed", speed);
                speed = speed.Trim();
                return double.Parse(speed, CultureInfo.InvariantCulture) * mult * (1.0 - DevFee * 0.01);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetNumber",
                    ex.Message + " | args => " + outdata + " | " + lookForEnd + " | " + lookForStart);
            }

            return 0;
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType);
            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes("{\"method\":\"getstat\"}\n");
                var client = new TcpClient("127.0.0.1", ApiPort);
                var nwStream = client.GetStream();
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                var bytesToRead = new byte[client.ReceiveBufferSize];
                var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                client.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ex.Message);
            }

            if (resp != null && resp.error == null)
            {
                ad.Speed = resp.result.Aggregate<Result, uint>(0, (current, t1) => current + t1.speed_sps);
                CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                if (ad.Speed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }
            }

            return ad;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }
    }
}
