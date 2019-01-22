using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class GMiner : Miner
    {
        private string AlgoName
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.ZHash:
                        return "equihash144_5";
                    case AlgorithmType.Beam:
                        return "equihash150_5";
                    default:
                        return "";
                }
            }
        }

        private string Personalization => "auto";

        public GMiner() : base("gminer")
        { }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 2 * 60 * 1000;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            var split = url.Split(':');
            var devs = string.Join(",", MiningSetup.DeviceIDs);
            LastCommandLine = $"-a {AlgoName} -s {split[0]} -n {split[1]} " +
                              $"-u {btcAdress}.{worker} -d {devs} --api {ApiPort}";

            ProcessHandle = _Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            ShutdownMiner();
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
            return null;
        }
    }
}
