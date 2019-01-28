using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class BMiner : Miner
    {
        public BMiner() : base("bminer")
        {
            ConectionType = NhmConectionType.NONE;
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 5 * 60 * 1000;
        }

        private string CreateCommandLine(string url, string btcAddress, string worker)
        {
            var user = GetUsername(btcAddress, worker);
            var cmd = $"-uri {MiningSetup.MinerName}://{user}@{url} -api 127.0.0.1:{ApiPort}";

            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.ZHash)
            {
                cmd += " -pers auto";
            }

            return cmd;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = CreateCommandLine(url, btcAdress, worker);

            ProcessHandle = _Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            ShutdownMiner(true);
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

        public override Task<ApiData> GetSummaryAsync()
        {
            return null;
        }
    }
}
