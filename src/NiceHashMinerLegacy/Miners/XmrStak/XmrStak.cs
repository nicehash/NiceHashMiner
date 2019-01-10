using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMiner.Miners.Parsing;
using NiceHashMiner.Miners.XmrStak.Configs;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.XmrStak
{
    public class XmrStak : Miner
    {
        private static readonly object _fileLock = new object();

        public XmrStak(string name = "")
            : base("XmrStak")
        {
            ConectionType = NhmConectionType.NONE;
            IsNeverHideMiningWindow = true;
            IsMultiType = true;
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 5 * 60 * 1000; // 5 minutes
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            ShutdownMiner();
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            return await GetSummaryCpuAsync("api.json", true);
        }

        protected override bool IsApiEof(byte third, byte second, byte last)
        {
            return second == 0x7d && last == 0x7d;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }

            var devConfigs = PrepareConfigFiles();
            LastCommandLine = CreateLaunchCommand(devConfigs, url, GetUsername(btcAdress, worker));

            var envs = new Dictionary<string, string>
            {
                { "XMRSTAK_NOWAIT", "1" }
            };

            ProcessHandle = _Start(envs);
        }

        private string CreateLaunchCommand(Dictionary<DeviceType, string> devConfigs, string url, string user)
        {
            var devs = "";
            if (devConfigs != null)
            {
                devs = devConfigs.Keys.Aggregate("", (current, dev) => current + $"--{dev.ToString().ToLower()} {devConfigs[dev]} ") +
                    DisableDevCmd(devConfigs.Keys);
            }

            return $"-o {url} -u {user} --currency {MiningSetup.MinerName} -i {ApiPort} " +
                   $"--use-nicehash -p x -r x {devs}";
        }

        private Dictionary<DeviceType, string> PrepareConfigFiles()
        {
            lock (_fileLock)
            {
                var configs = new Dictionary<DeviceType, string>();
                var types = new List<DeviceType>();
                foreach (var pair in MiningSetup.MiningPairs)
                {
                    if (!types.Contains(pair.Device.DeviceType)) types.Add(pair.Device.DeviceType);
                }

                foreach (var type in types)
                {
                    if (type == DeviceType.CPU)
                    {
                        var cpuPair = MiningSetup.MiningPairs.Find(p => p.Device.DeviceType == type);
                        var isHyperThreadingEnabled = cpuPair.CurrentExtraLaunchParameters.Contains("enable_ht=true");
                        var numTr = ExtraLaunchParametersParser.GetThreadsNumber(cpuPair);
                        var noPrefetch = ExtraLaunchParametersParser.GetNoPrefetch(cpuPair);
                        if (isHyperThreadingEnabled)
                        {
                            numTr /= 2;
                        }

                        // Fallback on classic config if haven't been able to open 
                        var configCpu = ParseJsonFile<XmrStakConfigCpu>(type) ?? new XmrStakConfigCpu(numTr);
                        if (configCpu.cpu_threads_conf.Count == 0)
                        {
                            // No thread count would prevent CPU from mining, so fill with estimates
                            // Otherwise use values set by xmr-stak/user
                            configCpu.InitCpuThreads(false, noPrefetch, false, isHyperThreadingEnabled);
                        }

                        configs[type] = WriteJsonFile(configCpu, type);
                    }
                    else
                    {
                        var ids = MiningSetup.MiningPairs.Where(p => p.Device.DeviceType == type)
                            .Select(p => p.Device.ID);

                        if (type == DeviceType.AMD)
                        {
                            var configGpu = ParseJsonFile<XmrStakConfigAmd>(type) ?? new XmrStakConfigAmd();
                            configGpu.SetupThreads(ids);
                            configs[type] = WriteJsonFile(configGpu, type);
                        }
                        else
                        {
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
        }

        #region Filename Helpers

        private string GetDevConfigFileName(DeviceType type)
        {
            var ids = MiningSetup.MiningPairs
                .Where(pair => pair.Device.DeviceType == type)
                .Select(pair => pair.Device.ID);
            return $"{type}_{string.Join(",", ids)}.txt";
        }

        #endregion

        #region Commandline helpers

        // Return command to disable unused devices
        private static string DisableDevCmd(ICollection<DeviceType> usedDevs)
        {
            var devTypes = new List<DeviceType>
            {
                DeviceType.AMD,
                DeviceType.CPU,
                DeviceType.NVIDIA
            };
            return devTypes.FindAll(d => !usedDevs.Contains(d))
                .Aggregate("", (current, dev) => current + $"--no{dev} ");
        }

        #endregion

        #region Benchmarking

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            return GetBenchmarkCommandLine(algorithm.NiceHashID, time, PrepareConfigFiles());
        }

        private string GetBenchmarkCommandLine(AlgorithmType algorithm, int time, Dictionary<DeviceType, string> devConfigs)
        {
            var url = Globals.GetLocationUrl(algorithm,
                Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], ConectionType);
            var user = GetUsername(Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName);

            BenchmarkTimeInSeconds = Math.Min(60, Math.Max(time, 10));
            
            return CreateLaunchCommand(devConfigs, url, user) + $" --benchmark 0 --benchwork {time} --benchwait 5";
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            if (!outdata.TryGetHashrateAfter("Benchmark Total:", out var hash)) return false;
            BenchmarkAlgorithm.BenchmarkSpeed = hash;
            return true;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        #endregion

        #region JSON Helpers

        private T ParseJsonFile<T>(DeviceType type = DeviceType.CPU, string filename = "", bool fallback = false)
            where T : new()
        {
            if (filename == "") filename = $"{type.ToString().ToLower()}.txt";
            var json = default(T);
            try
            {
                var file = File.ReadAllText(WorkingDirectory + filename);
                file = "{" + file + "}";
                json = JsonConvert.DeserializeObject<T>(file);
            }
            catch (FileNotFoundException)
            {
                Helpers.ConsolePrint(MinerTag(), $"Config file {filename} not found, attempting to generate");
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint(MinerTag(), e.ToString());
            }

            if (json == null)
            {
                // If from recursive call, don't try again
                if (fallback)
                {
                    Helpers.ConsolePrint(MinerTag(), "xmr-stak did not generate configs, using default values");
                    return new T();
                }

                var timer = new Stopwatch();

                Helpers.ConsolePrint(MinerTag(), "Launching xmr-stak to generate configs, this can take up to 30s");

                // Try running xmr-stak to create default configs
                var handle = BenchmarkStartProcess(GetBenchmarkCommandLine(MiningSetup.CurrentAlgorithmType, 10, null));

                timer.Start();
                try
                {
                    while (timer.Elapsed.TotalSeconds < 30)
                    {
                        if (File.Exists(WorkingDirectory + filename))
                            break;

                        Thread.Sleep(500);
                    }

                    handle.WaitForExit(1000);
                    handle.Kill();
                    handle.WaitForExit(20 * 1000);
                    handle.Close();
                }
                catch { }

                json = ParseJsonFile<T>(type, filename, true);
            }

            return json;
        }

        private string WriteJsonFile(object config, string filename, string genFrom = "", bool isNv = false)
        {
            var header = "// This config file was autogenerated by NHML.";
            if (genFrom != "")
            {
                header +=
                    $"\n// The values were filled from {genFrom}. If you wish to edit them, you should edit that file instead.";
            }

            if (isNv)
            {
                header +=
                    "\n// bsleep and bfactor are overriden by default for compatibility. You can disable this functionality by adding \"--keep-b\" to extra launch parameters.";
            }

            try
            {
                var confJson = JObject.FromObject(config);
                var writeStr = confJson.ToString();
                var start = writeStr.IndexOf("{");
                var end = writeStr.LastIndexOf("}");
                writeStr = writeStr.Substring(start + 1, end - 1);
                writeStr = header + "\n" + writeStr;
                File.WriteAllText(WorkingDirectory + filename, writeStr);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint(MinerTag(), e.ToString());
                Helpers.ConsolePrint(MinerTag(),
                    $"Config {filename} was unable to write for xmr-stak, mining may not work");
            }

            return filename;
        }

        private string WriteJsonFile(object config, DeviceType type)
        {
            return WriteJsonFile(config, GetDevConfigFileName(type), $"{type.ToString().ToLower()}.txt",
                type == DeviceType.NVIDIA);
        }

        #endregion
    }
}
