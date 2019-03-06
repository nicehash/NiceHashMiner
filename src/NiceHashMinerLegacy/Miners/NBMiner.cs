using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Miners.Parsing;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class NBMiner : Miner
    {
        private string AlgoName
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.GrinCuckaroo29:
                        return "cuckaroo";
                    case AlgorithmType.GrinCuckatoo31:
                        return "cuckatoo";
                    case AlgorithmType.DaggerHashimoto:
                        return "ethash";
                    default:
                        return "";
                }
            }
        }

        public NBMiner() : base("nbminer")
        { }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000;
        }

        private string GetStartCommand(string url, string btcAddress, string worker)
        {
            var user = GetUsername(btcAddress, worker);
            var devs = string.Join(",", MiningSetup.MiningPairs.Select(p => p.Device.ID));
            var cmd = $"-a {AlgoName} -o {url} -u {user} --api 127.0.0.1:{ApiPort} -d {devs} ";
            cmd += ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);

            return cmd;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = GetStartCommand(url, btcAdress, worker);

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

        public override async Task<ApiData> GetSummaryAsync()
        {
            return null;
        }
    }
}
