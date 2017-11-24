using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Enums;
using NiceHashMiner.Configs;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;

namespace NiceHashMiner.Miners
{
    public class XmrStak : Miner
    {
        private const string _configName = "config_nh.txt";
        public XmrStak(string name="")
            : base("XmrStak") {
            ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }
        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 5 * 60 * 1000;  // 5 minutes
        }

        protected string GetDevConfigFileName(DeviceType type) {
            var ids = MiningSetup.MiningPairs.Select(pair => pair.Device.ID).ToList();
            return $"{type}_{string.Join(",", ids)}.txt";
        }

        private string GetBenchConfigName() {
            var ids = MiningSetup.MiningPairs.Select(pair => pair.Device.ID).ToList();
            return $"bench_";
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        public override async Task<APIData> GetSummaryAsync() {
            return await GetSummaryCPUAsync("api.json", true);
        }

        protected override bool IsApiEof(byte third, byte second, byte last) {
            return second == 0x7d && last == 0x7d;
        }

        public override void Start(string url, string btcAdress, string worker) {
            if (!IsInit) {
                Helpers.ConsolePrint(MinerTAG(), "MiningSetup is not initialized exiting Start()");
                return;
            }

            var devConfigs = PrepareConfigFiles();
            LastCommandLine = CreateLaunchCommand(_configName, url, btcAdress, worker, devConfigs);

            ProcessHandle = _Start();
        }

        private string CreateLaunchCommand(string configName, string url, string btcAddress, string worker, Dictionary<DeviceType, string> devConfigs) {
            var username = GetUsername(btcAddress, worker);
            var devs = "";
            foreach (var dev in devConfigs.Keys) {
                if (string.IsNullOrEmpty(devConfigs[dev])) {
                    devs += $"--no{dev} ";
                }
                else {
                    devs += $"--{dev.ToString().ToLower()} {devConfigs[dev]} ";
                }
            }
            return $"-c {configName} --currency monero -o {url} -u {username}  -p x {devs} ";
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            string url = Globals.GetLocationURL(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], this.ConectionType);
            var configs = PrepareConfigFiles(true);
            return CreateLaunchCommand(GetBenchConfigName(), url, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim(), configs);
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

        protected virtual Dictionary<DeviceType, string> PrepareConfigFiles(bool bench = false) {
            var configs = new Dictionary<DeviceType, string>();
            var types = new List<DeviceType>();
            foreach (var pair in MiningSetup.MiningPairs) {
                if (!types.Contains(pair.Device.DeviceType)) types.Add(pair.Device.DeviceType);
            }

            foreach (var type in types) {
                if (type == DeviceType.CPU) {
                    var cpuPair = MiningSetup.MiningPairs.Find(p => p.Device.DeviceType == type);
                    var isHyperThreadingEnabled = cpuPair.CurrentExtraLaunchParameters.Contains("enable_ht=true");
                    var numTr = ExtraLaunchParametersParser.GetThreadsNumber(cpuPair);
                    var no_prefetch = ExtraLaunchParametersParser.GetNoPrefetch(cpuPair);
                    if (isHyperThreadingEnabled) {
                        numTr /= 2;
                    }
                    // Fallback on classic config if haven't been able to open 
                    var config = ParseJsonFile<XmrStakConfigCpu>($"{type.ToString().ToLower()}.txt") ?? new XmrStakConfigCpu(numTr);
                    if (config.cpu_threads_conf.Count == 0) {
                        // No thread count would prevent CPU from mining, so fill with estimates
                        // Otherwise use values set by xmr-stak/user
                        config.Inti_cpu_threads_conf(false, no_prefetch, false, isHyperThreadingEnabled);
                    }
                    configs[type] = WriteJsonFile(config, type);
                }

                var ids = MiningSetup.MiningPairs.Where(p => p.Device.DeviceType == type).Select(p => p.Device.ID);
            }

            return configs;
        }

        private T ParseJsonFile<T>(string filename, bool fallback = false) {
            if (fallback) return default(T);
            var json = default(T);
            try {
                var file = File.ReadAllText(WorkingDirectory + filename);
                json = JsonConvert.DeserializeObject<T>(file);
            }
            catch (Exception e) {
                Helpers.ConsolePrint(MinerTAG(), e.ToString());
            }
            if (json == null) {
                // Try running xmr-stak to create default configs
                var handle = BenchmarkStartProcess("");
                Thread.Sleep(2000);
                json = ParseJsonFile<T>(filename, true);
            }
            return json;
        }

        private string WriteJsonFile(object config, DeviceType type) {
            var filename = GetDevConfigFileName(type);
            try {
                var confJson = JObject.FromObject(config);
                string writeStr = confJson.ToString();
                int start = writeStr.IndexOf("{");
                int end = writeStr.LastIndexOf("}");
                writeStr = writeStr.Substring(start + 1, end - 1);
                System.IO.File.WriteAllText(WorkingDirectory + filename, writeStr);
            }
            catch (Exception e) {
                Helpers.ConsolePrint(MinerTAG(), e.ToString());
                Helpers.ConsolePrint(MinerTAG(), "Config was unable to write for xmr-stak, mining may not work");
            }
            return filename;
        }

        protected override NiceHashProcess _Start() {
            NiceHashProcess P = base._Start();

            var AffinityMask = MiningSetup.MiningPairs[0].Device.AffinityMask;
            if (AffinityMask != 0 && P != null)
                CPUID.AdjustAffinity(P.Id, AffinityMask);

            return P;
        }

        protected override Process BenchmarkStartProcess(string CommandLine) {
            Process BenchmarkHandle = base.BenchmarkStartProcess(CommandLine);

            var AffinityMask = MiningSetup.MiningPairs[0].Device.AffinityMask;
            if (AffinityMask != 0 && BenchmarkHandle != null)
                CPUID.AdjustAffinity(BenchmarkHandle.Id, AffinityMask);

            return BenchmarkHandle;
        }
    }
}
