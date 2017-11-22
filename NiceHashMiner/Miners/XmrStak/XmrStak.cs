using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Enums;
using NiceHashMiner.Configs;
using NiceHashMiner.Miners.Parsing;

namespace NiceHashMiner.Miners
{
    public class XmrStak : Miner
    {
        public XmrStak(string name="")
            : base("XmrStak") {
            ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }
        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 5 * 60 * 1000;  // 5 minutes
        }

        protected string GetConfigFileName() {
            var ids = MiningSetup.MiningPairs.Select(pair => pair.Device.ID).ToList();
            return $"config_{string.Join(",", ids)}.txt";
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
            string username = GetUsername(btcAdress, worker);
            LastCommandLine = GetConfigFileName();

            prepareConfigFile(url, username);

            ProcessHandle = _Start();
        }

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

        protected virtual void prepareConfigFile(string pool, string wallet) {
            if (this.MiningSetup.MiningPairs.Count > 0) {
                try {
                    bool IsHyperThreadingEnabled = this.MiningSetup.MiningPairs[0].CurrentExtraLaunchParameters.Contains("enable_ht=true");
                    int numTr = ExtraLaunchParametersParser.GetThreadsNumber(this.MiningSetup.MiningPairs[0]);
                    if (IsHyperThreadingEnabled) {
                        numTr /= 2;
                    }
                    var config = new XmrStackCPUMinerConfig(numTr, pool, wallet, this.APIPort);
                    var no_prefetch = ExtraLaunchParametersParser.GetNoPrefetch(MiningSetup.MiningPairs[0]);
                    //config.Inti_cpu_threads_conf(false, false, true, ComputeDeviceManager.Avaliable.IsHyperThreadingEnabled);
                    config.Inti_cpu_threads_conf(false, no_prefetch, false, IsHyperThreadingEnabled);
                    var confJson = JObject.FromObject(config);
                    string writeStr = confJson.ToString();
                    int start = writeStr.IndexOf("{");
                    int end = writeStr.LastIndexOf("}");
                    writeStr = writeStr.Substring(start + 1, end - 1);
                    System.IO.File.WriteAllText(WorkingDirectory + GetConfigFileName(), writeStr);
                } catch {
                }
            }
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
