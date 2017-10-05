using System;
using System.Net;
using System.Threading.Tasks;
using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Parsing;

namespace NiceHashMiner.Miners
{
    public class Xmrig : Miner
    {
        private int _benchmarkTimeWait = 120;

        public Xmrig() : base("Xmrig") {
            IsAPIReadException = true;
        }

        public override void Start(string url, string btcAdress, string worker) {
            LastCommandLine = GetStartCommand(url, btcAdress, worker);
            ProcessHandle = _Start();
        }

        private string GetStartCommand(string url, string btcAdress, string worker) {
            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.CPU);
            return $" -o {url} -u {btcAdress}.{worker}:x -k --nicehash {extras}";
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 60 * 1000 * 5;  // 5 min
        }

        public override Task<APIData> GetSummaryAsync() {
            return Task.FromResult(GetAPIReadExceptionStatus(MinerBaseType.Xmrig, AlgorithmType.CryptoNight));
        }

        #region Benchmark

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            var server = Globals.GetLocationURL(algorithm.NiceHashID,
                Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], 
                ConectionType);
            _benchmarkTimeWait = time;
            return GetStartCommand(server, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim())
                + " -l benchmark_log.txt --print-time=1";
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata) {
            Helpers.ConsolePrint(MinerTAG(), outdata);
            return false;
        }

        #endregion
    }
}