using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public static class ProspectorPlatforms
    {
        public static bool IsInit => NVPlatform >= 0 || AmdPlatform >= 0;
        public static int NVPlatform = -1;
        public static int AmdPlatform = -1;

        public static object Lock = new object();

        public static int PlatformForDeviceType(DeviceType type)
        {
            if (IsInit)
            {
                if (type == DeviceType.NVIDIA) return NVPlatform;
                if (type == DeviceType.AMD) return AmdPlatform;
            }
            return -1;
        }
    }

    public class Prospector : Miner
    {
#pragma warning disable IDE1006
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

        private class HashrateApiResponse
        {
            public string coin { get; set; }
            public string device { get; set; }
            public double rate { get; set; }
            public string time { get; set; }
        }
#pragma warning restore IDE1006

        private class ProspectorDatabase : SQLiteConnection
        {
            public ProspectorDatabase(string path)
                : base(new SQLitePlatformWin32(), path)
            { }

            public double QueryLastSpeed(string device)
            {
                try
                {
                    return Table<hashrates>().Where(x => x.device == device).OrderByDescending(x => x.time).Take(1).FirstOrDefault().rate;
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("PROSPECTORSQL", e.ToString());
                    return 0;
                }
            }

            public IEnumerable<hashrates> QuerySpeedsForSessionDev(int id, string device) 
            {
                try 
                {
                    return Table<hashrates>().Where(x => x.session_id == id && x.device == device);
                } 
                catch (Exception e) 
                {
                    Helpers.ConsolePrint("PROSPECTORSQL", e.ToString());
                    return new List<hashrates>();
                }
            }

            public sessions LastSession(string device)
            {
                try
                {
                    return Table<sessions>().LastOrDefault(s => QuerySpeedsForSessionDev(s.id, device).Any());
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("PROSPECTORSQL", e.ToString());
                    return new sessions();
                }
            }
        }

        private ProspectorDatabase _database;

        private int _benchmarkTimeWait;
        private const double DevFee = 0.01; // 1% devfee

        private const string PlatformStart = "platform ";
        private const string PlatformEnd = " - ";

        private const int apiPort = 42000;

        public Prospector()
            : base("Prospector")
        {
            ConectionType = NhmConectionType.STRATUM_TCP;
            IsNeverHideMiningWindow = true;
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 3600000; // 1hour
        }

        private string DeviceIDString(int id, DeviceType type)
        {
            var platform = 0;
            if (InitPlatforms())
            {
                platform = ProspectorPlatforms.PlatformForDeviceType(type);
            }
            else
            {
                // fallback
                Helpers.ConsolePrint(MinerTag(), "Failed to get platforms, falling back");
                if (ComputeDeviceManager.Available.HasNvidia && type != DeviceType.NVIDIA)
                    platform = 1;
            }
            return $"{platform}-{id}";
        }

        private string GetConfigFileName()
        {
            return $"config_{GetDeviceID()}.toml";
        }

        private void PrepareConfigFile(string pool, string wallet)
        {
            if (MiningSetup.MiningPairs.Count <= 0) return;
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("[general]");
                sb.AppendLine($"gpu-coin = \"{MiningSetup.MinerName}\"");
                sb.AppendLine($"default-username = \"{wallet}\"");
                sb.AppendLine("default-password = \"x\"");

                sb.AppendLine($"[pools.{MiningSetup.MinerName}]");
                sb.AppendLine($"url = \"{pool}\"");

                foreach (var dev in MiningSetup.MiningPairs)
                {
                    sb.AppendLine($"[gpus.{DeviceIDString(dev.Device.ID, dev.Device.DeviceType)}]");
                    sb.AppendLine("enabled = true");
                    sb.AppendLine($"label = \"{dev.Device.Name}\"");
                }

                sb.AppendLine("[cpu]");
                sb.AppendLine("enabled = false");

                File.WriteAllText(WorkingDirectory + GetConfigFileName(), sb.ToString());
            }
            catch { }
        }

        private bool InitPlatforms()
        {
            // We need to lock the platforms class 
            // If two devices start a prospector bench close together, the second one will delete the logs needed by the first
            // Once the first one has finished getting platform defs, platforms class will be init so the lock only lasts one line
            lock (ProspectorPlatforms.Lock)
            {
                if (ProspectorPlatforms.IsInit) return true;

                CleanAllOldLogs();
                using (var handle = BenchmarkStartProcess(" list-devices"))
                {
                    handle.Start();

                    handle.WaitForExit(20 * 1000); // 20 seconds
                }

                try
                {
                    var latestLogFile = "";
                    var dirInfo = new DirectoryInfo(WorkingDirectory + "logs\\");
                    foreach (var file in dirInfo.GetFiles())
                    {
                        latestLogFile = file.Name;
                        break;
                    }

                    if (File.Exists(dirInfo + latestLogFile))
                    {
                        var lines = File.ReadAllLines(dirInfo + latestLogFile);
                        foreach (var line in lines)
                        {
                            if (line == null) continue;
                            var lineLowered = line.ToLower();
                            if (!lineLowered.Contains(PlatformStart)) continue;
                            var platStart = lineLowered.IndexOf(PlatformStart);
                            var plat = lineLowered.Substring(platStart, line.Length - platStart);
                            plat = plat.Replace(PlatformStart, "");
                            plat = plat.Substring(0, plat.IndexOf(PlatformEnd));

                            if (!int.TryParse(plat, out var platIndex)) continue;
                            if (lineLowered.Contains("nvidia"))
                            {
                                Helpers.ConsolePrint(MinerTag(), "Setting nvidia platform: " + platIndex);
                                ProspectorPlatforms.NVPlatform = platIndex;
                                if (ProspectorPlatforms.AmdPlatform >= 0) break;
                            }
                            else if (lineLowered.Contains("amd"))
                            {
                                Helpers.ConsolePrint(MinerTag(), "Setting amd platform: " + platIndex);
                                ProspectorPlatforms.AmdPlatform = platIndex;
                                if (ProspectorPlatforms.NVPlatform >= 0) break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint(MinerTag(), e.ToString());
                }

                return ProspectorPlatforms.IsInit;
            }
        }

        private void CleanAllOldLogs()
        {
            // clean old logs
            try
            {
                var dirInfo = new DirectoryInfo(WorkingDirectory + "logs\\");
                if (dirInfo.Exists)
                {
                    foreach (var file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
            catch { }
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);

            var client = new WebClient();
            HashrateApiResponse[] resp = null;
            try
            {
                var url = $"http://localhost:{apiPort}/api/v0/hashrates";
                var data = client.OpenRead(url);
                var reader = new StreamReader(data);
                var s = await reader.ReadToEndAsync();
                data.Close();
                reader.Close();

                resp = JsonConvert.DeserializeObject<HashrateApiResponse[]>(s, Globals.JsonSettings);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + ex.Message);
            }

            if (resp != null)
            {
                ad.Speed = 0;
                foreach (var response in resp)
                {
                    if (response.coin == MiningSetup.MinerName)
                    {
                        ad.Speed += response.rate;
                        CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                    }
                }
                if (ad.Speed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }
            }

            return ad;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            LastCommandLine = GetStartupCommand(url, btcAdress, worker);

            ProcessHandle = _Start();
        }

        private string GetStartupCommand(string url, string btcAddress, string worker)
        {
            var username = GetUsername(btcAddress, worker);
            PrepareConfigFile(url, username);
            return "--config " + GetConfigFileName();
        }

        #region Benchmark

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            // Prospector can take a very long time to start up
            _benchmarkTimeWait = time + 60;
            // network stub
            var url = Globals.GetLocationUrl(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
                ConectionType);
            return GetStartupCommand(url, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim());
        }

        protected override void BenchmarkThreadRoutine(object commandLine)
        {
            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;
            
            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);

            var startTime = DateTime.Now;

            try
            {
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                Helpers.ConsolePrint(MinerTag(), "Benchmark should end in : " + _benchmarkTimeWait + " seconds");
                BenchmarkHandle = BenchmarkStartProcess((string) commandLine);
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
                        KillProspectorClaymoreMinerBase(imageName);
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
                BenchmarkHandle.WaitForExit(20 * 1000); // Wait up to 20s for exit
            }
            catch (Exception ex)
            {
                BenchmarkThreadRoutineCatch(ex);
            }
            finally
            {
                BenchmarkAlgorithm.BenchmarkSpeed = 0;

                if (_database == null)
                {
                    try
                    {
                        _database = new ProspectorDatabase(WorkingDirectory + "info.db");
                    }
                    catch (Exception e) { Helpers.ConsolePrint(MinerTag(), e.ToString()); }
                }
                
                var dev = MiningSetup.MiningPairs[0].Device;
                var devString = DeviceIDString(dev.ID, dev.DeviceType);

                var session = _database.LastSession(devString);
                if (session != null)
                {
                    var sessionStart = Convert.ToDateTime(session.start);
                    if (sessionStart >= startTime)
                    {
                        var hashrates = _database.QuerySpeedsForSessionDev(session.id, devString);

                        double speed = 0;
                        var speedRead = 0;
                        foreach (var hashrate in hashrates)
                        {
                            if (hashrate.coin == MiningSetup.MinerName && hashrate.rate > 0)
                            {
                                speed += hashrate.rate;
                                speedRead++;
                            }
                        }

                        BenchmarkAlgorithm.BenchmarkSpeed = (speed / Math.Max(1, speedRead)) * (1 - DevFee);
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

        #endregion Benchmarking
    }
}
