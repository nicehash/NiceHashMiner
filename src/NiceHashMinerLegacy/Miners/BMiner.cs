using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NiceHashMiner.Miners
{
    public class BMiner : Miner
    {
        public class EquiSpeedInfo
        {
            public double nonce_rate { get; set; }
            public double solution_rate { get; set; }
        }

        public class GenericSpeedInfo
        {
            public double hash_rate { get; set; }
        }

        private class JsonModel<T>
        {
            public class Solver<T>
            {
                public string algorithm { get; set; }
                public T speed_info { get; set; }
            }

            public List<Solver<T>> solvers { get; set; }
        }

        private bool IsEquihash
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.Beam:
                    case AlgorithmType.ZHash:
                        return true;
                    default:
                        return false;
                }
            }
        }

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

            var devs = string.Join(",", MiningSetup.MiningPairs.Select(p => p.Device).Select(d =>
            {
                var prefix = d.DeviceType == DeviceType.AMD ? "amd:" : "";
                return prefix + d.ID;
            }));

            var cmd = $"-uri {MiningSetup.MinerName}://{user}@{url} -api 127.0.0.1:{ApiPort} " +
                      $"-devices {devs}";

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

        internal double ParseApi(string data)
        {
            dynamic model = JsonConvert.DeserializeObject(data);
            var devs = model.devices as JObject ?? throw new ArgumentException();

            var hashrate = 0d;

            foreach (var dev in devs.PropertyValues())
            {
                if (IsEquihash)
                {
                    var obj = dev.ToObject<JsonModel<EquiSpeedInfo>>();
                    hashrate += obj.solvers[0].speed_info.solution_rate;
                }

                else
                {
                    var obj = dev.ToObject<JsonModel<GenericSpeedInfo>>();
                    hashrate += obj.solvers[0].speed_info.hash_rate;
                }
            }

            return hashrate;
        }
    }
}
