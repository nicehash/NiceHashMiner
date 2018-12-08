using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class Trex : Miner
    {
        public Trex() : base("trex")
        { }

        public override async Task<ApiData> GetSummaryAsync()
        {
            return null;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            var devices = string.Join(",", MiningSetup.MiningPairs.Select(p => p.Device.ID));
            LastCommandLine = $" -a skunk -d {devices} -o {url} -u {GetUsername(btcAdress, worker)} -p x";
            ProcessHandle = _Start();
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

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }
    }
}
