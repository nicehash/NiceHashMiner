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
using NiceHashMiner.Miners.Parsing;

namespace NiceHashMiner.Miners
{
    public class XmrStak : Miner
    {
        private const string _configName = "config_nh.txt";
        private const string _defConfigName = "config.txt";
        
        private int _benchmarkCount;
        private double _benchmarkSum;

        public XmrStak(string name="")
            : base("XmrStak") {
            ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
            TimeoutStandard = true;
        }
        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 5 * 60 * 1000;  // 5 minutes
        }

        #region Filename Helpers

        protected string GetDevConfigFileName(DeviceType type) {
            var ids = MiningSetup.MiningPairs.Where(pair => pair.Device.DeviceType == type).Select(pair => pair.Device.ID).ToList();
            return $"{type}_{string.Join(",", ids)}.txt";
        }

        private string GetBenchConfigName() {
            var dev = MiningSetup.MiningPairs[0].Device;
            return $"bench_{(int)dev.DeviceType}-{dev.ID}.txt";
        }

        protected override string GetLogFileName() {
            return $"{(int) MiningSetup.MiningPairs[0].Device.DeviceType}-{base.GetLogFileName()}";
        }

        #endregion

        #region Commandline helpers

        // Return command to disable unused devices
        private string DisableDevCmd(ICollection<DeviceType> usedDevs) {
            var devTypes = new List<DeviceType> {
                DeviceType.AMD,
                DeviceType.CPU,
                DeviceType.NVIDIA
            };
            return devTypes.FindAll(d => !usedDevs.Contains(d)).Aggregate("", (current, dev) => current + $"--no{dev} ");
        }

        private string DisableDevCmd(DeviceType usedDev) {
            return DisableDevCmd(new List<DeviceType> {usedDev});
        }

        #endregion

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

            var devConfigs = PrepareConfigFiles(url, btcAdress, worker);
            LastCommandLine = CreateLaunchCommand(_configName, devConfigs);

            ProcessHandle = _Start();
        }

        private string CreateLaunchCommand(string configName, Dictionary<DeviceType, string> devConfigs) {
            var devs = devConfigs.Keys.Aggregate("", (current, dev) => current + $"--{dev.ToString().ToLower()} {devConfigs[dev]} ");
            return $"-c {configName} {devs} {DisableDevCmd(devConfigs.Keys)}";
        }

        protected virtual Dictionary<DeviceType, string> PrepareConfigFiles(string url, string btcAddress, string worker, bool bench = false) {
            var configs = new Dictionary<DeviceType, string>();
            var types = new List<DeviceType>();
            foreach (var pair in MiningSetup.MiningPairs) {
                if (!types.Contains(pair.Device.DeviceType)) types.Add(pair.Device.DeviceType);
            }

            var configName = bench ? GetBenchConfigName() : _configName;
            var config = ParseJsonFile<XmrStakConfig>(filename: _defConfigName) ?? new XmrStakConfig();
            config.SetupPools(url, GetUsername(btcAddress, worker));
            config.httpd_port = APIPort;
            if (bench) {
                config.SetBenchmarkOptions(GetLogFileName());
            }
            WriteJsonFile(config, configName, _defConfigName);

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
                    var configCpu = ParseJsonFile<XmrStakConfigCpu>(type) ?? new XmrStakConfigCpu(numTr);
                    if (configCpu.cpu_threads_conf.Count == 0) {
                        // No thread count would prevent CPU from mining, so fill with estimates
                        // Otherwise use values set by xmr-stak/user
                        configCpu.Inti_cpu_threads_conf(false, no_prefetch, false, isHyperThreadingEnabled);
                    }
                    configs[type] = WriteJsonFile(configCpu, type);
                } else {
                    var ids = MiningSetup.MiningPairs.Where(p => p.Device.DeviceType == type).Select(p => p.Device.ID);

                    if (type == DeviceType.AMD) {
                        var configGpu = ParseJsonFile<XmrStakConfigAmd>(type) ?? new XmrStakConfigAmd();
                        configGpu.SetupThreads(ids);
                        configs[type] = WriteJsonFile(configGpu, type);
                    } else {
                        var keepBVals = MiningSetup.MiningPairs.Any(p =>
                            p.CurrentExtraLaunchParameters.Contains("--keep-b") && p.Device.DeviceType == type);
                        var configGpu = ParseJsonFile<XmrStakConfigNvidia>(type) ?? new XmrStakConfigNvidia();
                        // B values do not seem to work on many nv setups, workaround by forcing higher vals unless user opts out
                        configGpu.SetupThreads(ids);
                        if (!keepBVals) configGpu.OverrideBVals();
                        configs[type] = WriteJsonFile(configGpu, type);
                    }
                }
            }

            return configs;
        }

        #region Benchmarking

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            var url = Globals.GetLocationURL(algorithm.NiceHashID, ConfigManager.GeneralConfig.ServiceLocations[0].ServiceLocation, this.ConectionType);
            var configs = PrepareConfigFiles(url, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim(), true);
            _benchmarkCount = 0;
            _benchmarkSum = 0;
            BenchmarkTimeInSeconds = Math.Max(time, 60);
            CleanOldLogs();
            return CreateLaunchCommand(GetBenchConfigName(), configs);
        }

        protected override void FinishUpBenchmark() {
            BenchmarkAlgorithm.BenchmarkSpeed = _benchmarkSum / Math.Max(1, _benchmarkCount);
        }

        protected override bool BenchmarkParseLine(string outdata) {
            if (!outdata.Contains("Totals:")) return false;

            var speeds = outdata.Split();
            foreach (var s in speeds) {
                if (!double.TryParse(s, out var speed)) continue;
                _benchmarkSum += speed;
                _benchmarkCount++;
                break;
            }
            return false;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }

        #endregion

        #region JSON Helpers

        private T ParseJsonFile<T>(DeviceType type = DeviceType.CPU, string filename = "", bool fallback = false) {
            if (filename == "") filename = $"{type.ToString().ToLower()}.txt";
            var json = default(T);
            try {
                var file = File.ReadAllText(WorkingDirectory + filename);
                file = "{" + file + "}";
                json = JsonConvert.DeserializeObject<T>(file);
            }
            catch (Exception e) {
                if (e is FileNotFoundException) {
                    Helpers.ConsolePrint(MinerTAG(), $"Config file {filename} not found, attempting to generate");
                }
                else {
                    Helpers.ConsolePrint(MinerTAG(), e.ToString());
                }
            }
            if (json == null) {
                // If from recursive call, don't try again
                if (fallback) return default(T);
                if (!File.Exists(WorkingDirectory + _defConfigName)) {
                    // Exception since xmr-stak won't passively generate general config
                    var config = new XmrStakConfig();
                    // Dummy values for pools, won't load with an empty pool list
                    var url = Globals.GetLocationURL(AlgorithmType.CryptoNight,
                        ConfigManager.GeneralConfig.ServiceLocations[0].ServiceLocation, this.ConectionType);
                    var worker = GetUsername(Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim());
                    config.SetupPools(url, worker);
                    WriteJsonFile(config, filename);
                }
                if (typeof(T) != typeof(XmrStakConfig)) {
                    // Try running xmr-stak to create default configs
                    var handle = BenchmarkStartProcess(DisableDevCmd(type) + " --generate-configs ");
                    var timer = new Stopwatch();
                    timer.Start();
                    handle.Start();
                    try {
                        if (!handle.WaitForExit(20 * 1000)) {
                            handle.Kill(); // Should have exited already if using right xmr-stak ver
                            handle.WaitForExit(20 * 1000);
                            handle.Close();
                        }
                    }
                    catch { }
                    json = ParseJsonFile<T>(type, filename, true);
                }
            }
            return json;
        }

        private string WriteJsonFile(object config, string filename, string genFrom = "", bool isNv = false) {
            var header = "// This config file was autogenerated by NHML.";
            if (genFrom != "") {
                header +=
                    $"\n// The values were filled from {genFrom}. If you wish to edit them, you should edit that file instead.";
            }
            if (isNv) {
                header +=
                    "\n// bsleep and bfactor are overriden by default for compatibility. You can disable this functionality by adding \"--keep-b\" to extra launch parameters.";
            }
            try {
                var confJson = JObject.FromObject(config);
                var writeStr = confJson.ToString();
                var start = writeStr.IndexOf("{");
                var end = writeStr.LastIndexOf("}");
                writeStr = writeStr.Substring(start + 1, end - 1);
                writeStr = header + "\n" + writeStr;
                File.WriteAllText(WorkingDirectory + filename, writeStr);
            } catch (Exception e) {
                Helpers.ConsolePrint(MinerTAG(), e.ToString());
                Helpers.ConsolePrint(MinerTAG(), $"Config {filename} was unable to write for xmr-stak, mining may not work");
            }
            return filename;
        }

        private string WriteJsonFile(object config, DeviceType type) {
            return WriteJsonFile(config, GetDevConfigFileName(type), $"{type.ToString().ToLower()}.txt", type == DeviceType.NVIDIA);
        }

        #endregion
    }
}
