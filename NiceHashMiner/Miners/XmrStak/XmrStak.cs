using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Miners
{
    public abstract class XmrStak : Miner
    {
        protected XmrStak(string name)
            : base(name) {
            ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        protected abstract void prepareConfigFile(string pool, string wallet);

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
    }
}
