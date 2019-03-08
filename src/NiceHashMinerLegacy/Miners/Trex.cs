using Newtonsoft.Json;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Interfaces;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners
{
    public class Trex : Miner
    {
        private double _benchHashes;
        private int _benchIters;

        private class ApiSummary : IApiResult
        {
            [JsonProperty("hashrate")]
            public double? TotalHashrate { get; set; }
        }

        public Trex() : base("trex")
        {
            TimeoutStandard = true;
        }

        public override Task<ApiData> GetSummaryAsync()
        {
            return GetHttpSummaryAsync<ApiSummary>("summary");
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            var devices = string.Join(",", MiningSetup.MiningPairs.Select(p => p.Device.ID));
            LastCommandLine = $" -a {MiningSetup.MinerName} -d {devices} -o {url} " +
                              $"-u {GetUsername(btcAdress, worker)} -p x " +
                              $"--api-bind-http 127.0.0.1:{ApiPort}";
            ProcessHandle = _Start();
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            _benchHashes = 0;
            _benchIters = 0;
            var devices = string.Join(",", MiningSetup.MiningPairs.Select(p => p.Device.ID));
            return $" -a {algorithm.MinerName} -B -d {devices}";
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            if (!outdata.TryGetHashrateAfter("Total:", out var hashrate) ||
                hashrate <= 0)
            {
                return false;
            }

            _benchHashes += hashrate;
            _benchIters++;

            return false;
        }

        protected override void BenchmarkThreadRoutineFinish()
        {
            if (_benchIters != 0 && BenchmarkAlgorithm != null)
            {
                BenchmarkAlgorithm.BenchmarkSpeed = _benchHashes / _benchIters;
                if (BenchmarkAlgorithm.NiceHashID == AlgorithmType.X16R)
                {
                    // Quick adjustment, x16r speeds are overestimated by around 3.5
                    BenchmarkAlgorithm.BenchmarkSpeed /= 3.5;
                }
            }

            base.BenchmarkThreadRoutineFinish();
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            ShutdownMiner();
        }
    }
}
