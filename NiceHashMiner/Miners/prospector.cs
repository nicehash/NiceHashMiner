using Newtonsoft.Json.Linq;
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
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using SQLite.Net.Attributes;

namespace NiceHashMiner.Miners
{
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

        private class ProspectorDatabase : SQLiteConnection
        {
            public ProspectorDatabase(string path)
                : base(new SQLitePlatformWin32(), path) { }

            public double QuerySpeed(string device) {
                try {
                    return Table<hashrates>().Where(x => x.device == device).OrderByDescending(x => x.time).Take(1).FirstOrDefault().rate;
                } catch (Exception e) {
                    Helpers.ConsolePrint("PROSPECTORSQL", e.ToString());
                    return 0;
                }
            }
        }

        private ProspectorDatabase database;

        public Prospector()
            : base("Prospector") {
            this.ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 3600000; // 1hour
        }

        private string deviceIDString(int id) {
            return String.Format("(0:{0})", id);
        }

        private string GetConfigFileName() {
            return String.Format("config_{0}.json", this.MiningSetup.MiningPairs[0].Device.ID);
        }

        private void prepareConfigFile(string pool, string wallet) {
            if (this.MiningSetup.MiningPairs.Count > 0) {
                try {
                    var confJson = new JObject();
                    var poolsJson = new JObject();
                    var sigtJson = new JObject();
                    var devicesJson = new JObject();

                    sigtJson.Add("url", pool);
                    sigtJson.Add("username", wallet);
                    sigtJson.Add("password", "x");
                    poolsJson.Add("sigt", sigtJson);
                    confJson.Add("pools", poolsJson);

                    foreach (var dev in MiningSetup.MiningPairs) {
                        var devJson = new JObject();
                        devJson.Add("enabled", true);
                        devJson.Add("name", dev.Device.Name);

                        devicesJson.Add(deviceIDString(dev.Device.ID), devJson);
                    }

                    var cpuJson = new JObject();
                    cpuJson.Add("enabled", false);
                    cpuJson.Add("label", "CPU");

                    confJson.Add("gpus", devicesJson);
                    confJson.Add("cpu", cpuJson);

                    confJson.Add("gpu-coin", "sigt");

                    string writeStr = confJson.ToString();
                    int start = writeStr.IndexOf("{");
                    int end = writeStr.LastIndexOf("}");
                    System.IO.File.WriteAllText(WorkingDirectory + GetConfigFileName(), writeStr);
                } catch {
                }
            }
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        public override APIData GetSummary() {
            APIData ad = new APIData(MiningSetup.CurrentAlgorithmType);
            if (database == null) {
                try {
                    database = new ProspectorDatabase(WorkingDirectory + "info.db");
                } catch (Exception e) { Helpers.ConsolePrint(MinerTAG(), e.ToString()); }
            } else {
                foreach (var pair in MiningSetup.MiningPairs) {
                    if (database != null) {
                        ad.Speed += database.QuerySpeed(deviceIDString(pair.Device.ID));
                    }
                }
            }
            // check if speed zero
            if (ad.Speed == 0) _currentMinerReadStatus = MinerAPIReadStatus.READ_SPEED_ZERO;

            return ad;
        }

        public override void Start(string url, string btcAdress, string worker) {
            if (!IsInit) {
                Helpers.ConsolePrint(MinerTAG(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            string username = GetUsername(btcAdress, worker);
            LastCommandLine = "--config " + GetConfigFileName();

            prepareConfigFile(url, username);

            ProcessHandle = _Start();
        }

        // doesn't work stubs
        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            string url = Globals.GetLocationURL(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], this.ConectionType);
            prepareConfigFile(url, Globals.DemoUser);
            return "benchmark_mode " + GetConfigFileName();
        }

        protected override bool BenchmarkParseLine(string outdata) {
            if (outdata.Contains("Total:")) {
                string toParse = outdata.Substring(outdata.IndexOf("Total:")).Replace("Total:", "").Trim();
                var strings = toParse.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in strings) {
                    double lastSpeed = 0;
                    if (double.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out lastSpeed)) {
                        Helpers.ConsolePrint("BENCHMARK " + MinerTAG(), "double.TryParse true. Last speed is" + lastSpeed.ToString());
                        BenchmarkAlgorithm.BenchmarkSpeed = Helpers.ParseDouble(s);
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }
    }
}
