using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;
using System.Net;
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using SQLite.Net.Attributes;

namespace NiceHashMiner.Miners
{
    public static class ProspectorPlatforms
    {
        public static bool IsInit {
            get {
                return NVPlatform >= 0 || AMDPlatform >= 0;
            }
        }
        public static int NVPlatform = -1;
        public static int AMDPlatform = -1;

        public static int PlatformForDeviceType(DeviceType type) {
            if (IsInit) {
                if (type == DeviceType.NVIDIA) return NVPlatform;
                if (type == DeviceType.AMD) return AMDPlatform;
            }
            return -1;
        }
    }
    public class Prospector : Miner
    {
        private class hashrates
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            public int session_id { get; set; }
            public string coin { get; set; }
            public string device { get; set; }
            public int time { get; set; }
            public double rate { get; set; }
        }

        private class sessions
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            public string start { get; set; }
        }

        private class ProspectorDatabase : SQLiteConnection
        {
            public ProspectorDatabase(string path)
                : base(new SQLitePlatformWin32(), path) { }

            public double QueryLastSpeed(string device) {
                try {
                    return Table<hashrates>().Where(x => x.device == device).OrderByDescending(x => x.time).Take(1).FirstOrDefault().rate;
                } catch (Exception e) {
                    Helpers.ConsolePrint("PROSPECTORSQL", e.ToString());
                    return 0;
                }
            }

            public IEnumerable<hashrates> QuerySpeedsForSession(int id) {
                try {
                    return Table<hashrates>().Where(x => x.session_id == id);
                } catch (Exception e) {
                    Helpers.ConsolePrint("PROSPECTORSQL", e.ToString());
                    return new List<hashrates>();
                }
            }

            public sessions LastSession() {
                try {
                    return Table<sessions>().LastOrDefault();
                } catch (Exception e) {
                    Helpers.ConsolePrint("PROSPECTORSQL", e.ToString());
                    return new sessions();
                }
            }
        }

        private ProspectorDatabase database;

        private int benchmarkTimeWait;
        private const double DevFee = 0.01;  // 1% devfee

        private const string platformStart = "platform ";
        private const string platformEnd = " - ";

        private const int apiPort = 42000;

        public Prospector()
            : base("Prospector") {
            this.ConectionType = NHMConectionType.STRATUM_TCP;
            IsNeverHideMiningWindow = true;
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 3600000; // 1hour
        }

        private string deviceIDString(int id, DeviceType type) {
            var platform = 0;
            if (InitPlatforms()) {
                platform = ProspectorPlatforms.PlatformForDeviceType(type);
            } else {  // fallback
                Helpers.ConsolePrint(MinerTAG(), "Failed to get platforms, falling back");
                if (ComputeDeviceManager.Avaliable.HasNVIDIA && type != DeviceType.NVIDIA)
                    platform = 1;
            }
            return String.Format("{0}-{1}", platform, id);
        }

        private string GetConfigFileName() {
            return String.Format("config_{0}.toml", this.MiningSetup.MiningPairs[0].Device.ID);
        }

        private void prepareConfigFile(string pool, string wallet) {
            if (this.MiningSetup.MiningPairs.Count > 0) {
                try {
                    var sb = new StringBuilder();

                    sb.AppendLine("[general]");
                    sb.AppendLine(String.Format("gpu-coin = \"{0}\"", MiningSetup.MinerName));
                    sb.AppendLine(String.Format("default-username = \"{0}\"", wallet));
                    sb.AppendLine("default-password = \"x\"");

                    sb.AppendLine(String.Format("[pools.{0}]", MiningSetup.MinerName));
                    sb.AppendLine(String.Format("url = \"{0}\"", pool));

                    foreach (var dev in MiningSetup.MiningPairs) {
                        sb.AppendLine(String.Format("[gpus.{0}]", deviceIDString(dev.Device.ID, dev.Device.DeviceType)));
                        sb.AppendLine("enabled = true");
                        sb.AppendLine(String.Format("label = \"{0}\"", dev.Device.Name));
                    }

                    sb.AppendLine("[cpu]");
                    sb.AppendLine("enabled = false");

                    System.IO.File.WriteAllText(WorkingDirectory + GetConfigFileName(), sb.ToString());
                } catch {
                }
            }
        }

        private bool InitPlatforms() {
            if (ProspectorPlatforms.IsInit) return true;

            CleanAllOldLogs();
            var handle = BenchmarkStartProcess(" list-devices");
            handle.Start();

            handle.WaitForExit(20 * 1000);  // 20 seconds
            handle = null;

            try {
                string latestLogFile = "";
                var dirInfo = new DirectoryInfo(this.WorkingDirectory + "logs\\");
                foreach (var file in dirInfo.GetFiles()) {
                    latestLogFile = file.Name;
                    break;
                }
                if (File.Exists(dirInfo + latestLogFile)) {
                    var lines = File.ReadAllLines(dirInfo + latestLogFile);
                    foreach (var line in lines) {
                        if (line != null) {
                            string lineLowered = line.ToLower();
                            if (lineLowered.Contains(platformStart)) {
                                int platStart = lineLowered.IndexOf(platformStart);
                                string plat = lineLowered.Substring(platStart, line.Length - platStart);
                                plat = plat.Replace(platformStart, "");
                                plat = plat.Substring(0, plat.IndexOf(platformEnd));

                                int platIndex = -1;
                                if (int.TryParse(plat, out platIndex)) {
                                    if (lineLowered.Contains("nvidia")) {
                                        Helpers.ConsolePrint(MinerTAG(), "Setting nvidia platform: " + platIndex);
                                        ProspectorPlatforms.NVPlatform = platIndex;
                                        if (ProspectorPlatforms.AMDPlatform >= 0) break;
                                    } else if (lineLowered.Contains("amd")) {
                                        Helpers.ConsolePrint(MinerTAG(), "Setting amd platform: " + platIndex);
                                        ProspectorPlatforms.AMDPlatform = platIndex;
                                        if (ProspectorPlatforms.NVPlatform >= 0) break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception e) { Helpers.ConsolePrint(MinerTAG(), e.ToString()); }

            return ProspectorPlatforms.IsInit;
        }
        protected void CleanAllOldLogs() {
            // clean old logs
            try {
                var dirInfo = new DirectoryInfo(this.WorkingDirectory + "logs\\");
                if (dirInfo != null && dirInfo.Exists) {
                    foreach (FileInfo file in dirInfo.GetFiles()) {
                        file.Delete();
                    }
                }
            } catch { }
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }
        
        private class HashrateApiResponse
        {
            public string coin { get; set; }
            public string device { get; set; }
            public double rate { get; set; }
            public string time { get; set; }
        }

        public override APIData GetSummary() {
            _currentMinerReadStatus = MinerAPIReadStatus.NONE;
            APIData ad = new APIData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);

            WebClient client = new WebClient();
            HashrateApiResponse[] resp = null;
            try {
                string url = String.Format("http://localhost:{0}/api/v0/hashrates", apiPort);
                Stream data = client.OpenRead(url);
                StreamReader reader = new StreamReader(data);
                string s = reader.ReadToEnd();
                data.Close();
                reader.Close();

                resp = JsonConvert.DeserializeObject<HashrateApiResponse[]>(s, Globals.JsonSettings);
            } catch (Exception ex) {
                Helpers.ConsolePrint(this.MinerTAG(), "GetSummary exception: " + ex.Message);
            }

            if (resp != null) {
                ad.Speed = 0;
                foreach (var response in resp) {
                    if (response.coin == MiningSetup.MinerName) {
                        ad.Speed += response.rate;
                        _currentMinerReadStatus = MinerAPIReadStatus.GOT_READ;
                    }
                }
                if (ad.Speed == 0) {
                    _currentMinerReadStatus = MinerAPIReadStatus.READ_SPEED_ZERO;
                }
            }

            return ad;
        }

        public override void Start(string url, string btcAdress, string worker) {
            if (!IsInit) {
                Helpers.ConsolePrint(MinerTAG(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            LastCommandLine = GetStartupCommand(url, btcAdress, worker);

            ProcessHandle = _Start();
        }

        private string GetStartupCommand(string url, string btcAddress, string worker) {
            string username = GetUsername(btcAddress, worker);
            prepareConfigFile(url, username);
            return "--config " + GetConfigFileName();
        }

        #region Benchmark

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            // Prospector can take a very long time to start up
            benchmarkTimeWait = time + 60;
            // network stub
            string url = Globals.GetLocationURL(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], this.ConectionType);
            return GetStartupCommand(url, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim());
        }
        private bool IsActiveProcess(int pid) {
            try {
                return Process.GetProcessById(pid) != null;
            } catch {
                return false;
            }
        }

        protected override void BenchmarkThreadRoutine(object CommandLine) {
            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);

            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;

            var startTime = DateTime.Now;

            try {
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                Helpers.ConsolePrint(MinerTAG(), "Benchmark should end in : " + benchmarkTimeWait + " seconds");
                BenchmarkHandle = BenchmarkStartProcess((string)CommandLine);
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
                        KillProspectorClaymoreMinerBase(imageName);
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
                BenchmarkHandle.WaitForExit(20 * 1000);  // Wait up to 20s for exit
            } catch (Exception ex) {
                BenchmarkThreadRoutineCatch(ex);
            } finally {
                BenchmarkAlgorithm.BenchmarkSpeed = 0;

                if (database == null) {
                    try {
                        database = new ProspectorDatabase(WorkingDirectory + "info.db");
                    } catch (Exception e) { Helpers.ConsolePrint(MinerTAG(), e.ToString()); }
                }

                var session = database.LastSession();
                var sessionStart = Convert.ToDateTime(session.start);
                if (sessionStart < startTime) {
                    throw new Exception("Session not recorded!");
                }

                var hashrates = database.QuerySpeedsForSession(session.id);

                double speed = 0;
                int speedRead = 0;
                foreach (var hashrate in hashrates) {
                    if (hashrate.coin == MiningSetup.MinerName && hashrate.rate > 0) {
                        speed += hashrate.rate;
                        speedRead++;
                    }
                }

                BenchmarkAlgorithm.BenchmarkSpeed = (speed / speedRead) * (1 - DevFee);

                BenchmarkThreadRoutineFinish();
            }
        }

        // stub benchmarks read from file
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata) {
            Helpers.ConsolePrint("BENCHMARK", outdata);
            return false;
        }
        #endregion Benchmarking
    }
}
