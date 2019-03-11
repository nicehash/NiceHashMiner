using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners.Parsing;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners
{
    // NOTE: GMiner will NOT run if the VS debugger is attached to NHML. 
    // Detach the debugger to use GMiner.
    public class GMiner : Miner
    {
        private const double DevFee = 2.0;

        private int _benchIters;
        private double _benchHashes;
        private int _targetBenchIters;

#pragma warning disable 0649
        private class JsonModel : IApiResult
        {
            public class DeviceEntry
            {
                public int gpu_id;
                public double speed;
            }

            public List<DeviceEntry> devices;

            public double? TotalHashrate => devices?.Sum(d => d.speed);
        }
#pragma warning restore 0649

        private string AlgoName
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.ZHash:
                        return "144_5";
                    case AlgorithmType.Beam:
                        return "150_5";
                    case AlgorithmType.GrinCuckaroo29:
                        return "grin29";
                    case AlgorithmType.GrinCuckatoo31:
                        return "grin31";
                    default:
                        return "";
                }
            }
        }

        public GMiner() : base("gminer")
        {
            ConectionType = NhmConectionType.NONE;
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 2 * 60 * 1000;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = CreateCommandLine(url, btcAdress, worker);

            ProcessHandle = _Start();
        }

        private string CreateCommandLine(string url, string btcAddress, string worker)
        {
            var split = url.Split(':');
            // FOR AMD BEAM
            var amdStart = AvailableDevices.NumDetectedNvDevs;
            var devs = string.Join(" ", MiningSetup.MiningPairs.Select(pair => {
                var busID = pair.Device.DeviceType == DeviceType.NVIDIA ? pair.Device.IDByBus : amdStart + pair.Device.IDByBus;
                return busID.ToString();
            }));
            var cmd = $"-a {AlgoName} -s {split[0]} -n {split[1]} " +
                              $"-u {btcAddress}.{worker} -d {devs} --api {ApiPort} -w 0"; // worker doesn't fix instant start/stop

            cmd += ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);

            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.ZHash)
            {
                cmd += " --pers auto";
            }

            return cmd;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            // TODO fixes instant start/stop
            ShutdownMiner(false);
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            _benchHashes = 0;
            _benchIters = 0;
            _targetBenchIters = Math.Max(1, (int) Math.Floor(time / 30d));
            
            var url = GetServiceUrl(algorithm.NiceHashID);
            var btc = Globals.GetBitcoinUser();
            var worker = ConfigManager.GeneralConfig.WorkerName.Trim();

            return CreateCommandLine(url, btc, worker);
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            if (!outdata.TryGetHashrateAfter("Total Speed:", out var hashrate) ||
                hashrate <= 0)
            {
                return false;
            }

            _benchHashes += hashrate;
            _benchIters++;

            return _benchIters >= _targetBenchIters;
        }

        protected override void BenchmarkThreadRoutineFinish()
        {
            if (_benchIters != 0 && BenchmarkAlgorithm != null)
            {
                BenchmarkAlgorithm.BenchmarkSpeed = (_benchHashes / _benchIters) * (1 - DevFee * 0.01);
            }

            base.BenchmarkThreadRoutineFinish();
        }

        public override Task<ApiData> GetSummaryAsync()
        {
            return GetHttpSummaryAsync<JsonModel>("stat");
        }
    }
}
