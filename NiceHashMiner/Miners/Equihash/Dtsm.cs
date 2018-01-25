using NiceHashMiner.Enums;
using System;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners
{
    public class Dtsm : Miner
    {
        public Dtsm() : base("dtsm")
        {
            ConectionType = NhmConectionType.NONE;
        }
        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = GetStartCommand(url, btcAdress, worker);
            ProcessHandle = _Start();
        }

        private string GetStartCommand(string url, string btcAddress, string worker)
        {
            var urls = url.Split(':');
            var server = urls.Length > 0 ? urls[0] : "";
            var port = urls.Length > 1 ? urls[1] : "";
            return $" --server {server} --port {port} --user {btcAddress}.{worker} ";
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            throw new NotImplementedException();
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            throw new NotImplementedException();
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            throw new NotImplementedException();
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType);

            return ad;
        }
    }
}
