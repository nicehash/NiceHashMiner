using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class GMiner : Miner
    {
        private readonly HttpClient _httpClient;

        private class JsonModel
        {
            public class DeviceEntry
            {
                public int gpu_id;
                public double speed;
            }

            public List<DeviceEntry> devices;
        }

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

        public GMiner() : base("gminer")
        {
            ConectionType = NhmConectionType.NONE;
            _httpClient = new HttpClient();
        }

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

            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.ZHash)
            {
                LastCommandLine += " --pers auto";
            }

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
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            var api = new ApiData(MiningSetup.CurrentAlgorithmType);
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{ApiPort}/stat");
                var summary = JsonConvert.DeserializeObject<JsonModel>(result);
                api.Speed = summary.devices.Sum(d => d.speed);
                CurrentMinerReadStatus =
                    api.Speed <= 0 ? MinerApiReadStatus.READ_SPEED_ZERO : MinerApiReadStatus.GOT_READ;
            }
            catch (Exception e)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
                Helpers.ConsolePrint(MinerTag(), e.Message);
            }

            return api;
        }
    }
}
