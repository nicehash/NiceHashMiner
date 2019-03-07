using NiceHashMiner.Algorithms;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners.Parsing;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner.Miners
{
    public class NBMiner : VanillaProcessMiner
    {
        private class JsonModel : IApiResult
        {
            public class MinerModel
            {
                public class DeviceModel
                {
                    public double hashrate { get; set; }
                }

                public List<DeviceModel> devices { get; set; }

                public double total_hashrate { get; set; }
            }

            public MinerModel miner { get; set; }

            public double? TotalHashrate => miner?.total_hashrate;
        }

        private double _benchHashes;
        private int _benchIters;
        private int _targetBenchIters;

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

        private double DevFee
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.GrinCuckaroo29:
                    case AlgorithmType.GrinCuckatoo31:
                        return 2.0;
                    case AlgorithmType.DaggerHashimoto:
                        return 0.65;
                    default:
                        return 0;
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
            var devs = string.Join(",", MiningSetup.MiningPairs.Select(p => p.Device.IDByBus));
            var cmd = $"-a {AlgoName} -o {url} -u {user} --api 127.0.0.1:{ApiPort} -d {devs} -RUN ";
            cmd += ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);

            return cmd;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = GetStartCommand(url, btcAdress, worker);

            _Start();
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            _benchHashes = 0;
            _benchIters = 0;
            _targetBenchIters = Math.Max(1, (int) Math.Floor(time / 20d));

            var url = GetServiceUrl(algorithm.NiceHashID);
            var btc = Globals.GetBitcoinUser();
            var worker = ConfigManager.GeneralConfig.WorkerName.Trim();

            return GetStartCommand(url, btc, worker);
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            var id = MiningSetup.MiningPairs.First().Device.IDByBus;
            if (!outdata.TryGetHashrateAfter($" - {id}: ", out var hashrate) ||
                hashrate <= 0)
            {
                return false;
            }

            _benchHashes += hashrate;
            _benchIters++;

            return _benchIters >= _targetBenchIters;
        }

        protected override void FinishUpBenchmark()
        {
            if (_benchIters != 0 && BenchmarkAlgorithm != null)
            {
                BenchmarkAlgorithm.BenchmarkSpeed = (_benchHashes / _benchIters) * (1 - DevFee * 0.01);
            }
        }

        public override Task<ApiData> GetSummaryAsync()
        {
            return GetHttpSummaryAsync<JsonModel>("api/v1/status");
        }
    }
}
