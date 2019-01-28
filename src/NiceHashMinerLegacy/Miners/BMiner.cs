using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Miners
{
    public class BMiner : Miner
    {
        private class EquiSpeedInfo
        {
            public double nonce_rate { get; set; }
            public double solution_rate { get; set; }
        }

        private class GenericSpeedInfo
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

        private readonly HttpClient _httpClient;

        private Process _process;

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
            _httpClient = new HttpClient();
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
                      $"-devices {devs} -watchdog=false";

            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.ZHash)
            {
                cmd += " -pers auto";
            }

            return cmd;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = CreateCommandLine(url, btcAdress, worker);

            _Start();
        }

        // BMiner throws a fit if started with NiceHashProcess so use System.Diagnostics.Process instead
        // WARNING ProcessHandle will be null so do not call methods that access it (currently _Stop() is the only
        // one and it is overridden here)
        // TODO is NiceHashProcess necessary or can we use System.Diagnostics.Process everywhere?
        protected override NiceHashProcess _Start(IReadOnlyDictionary<string, string> envVariables = null)
        {
            if (_isEnded)
            {
                return null;
            }

            _process = new Process();

            var nhmlDirectory = Directory.GetCurrentDirectory();
            _process.StartInfo.WorkingDirectory = System.IO.Path.Combine(nhmlDirectory, WorkingDirectory);
            _process.StartInfo.FileName = System.IO.Path.Combine(nhmlDirectory, Path);
            _process.StartInfo.Arguments = LastCommandLine;
            _process.Exited += (sender, args) =>
            {
                Miner_Exited();
            };
            _process.EnableRaisingEvents = true;
            _process.StartInfo.CreateNoWindow = ConfigManager.GeneralConfig.HideMiningWindows;

            _process.StartInfo.UseShellExecute = false;

            try
            {
                if (_process.Start())
                {
                    IsRunning = true;

                    _currentPidData = new MinerPidData
                    {
                        MinerBinPath = Path,
                        Pid = _process.Id
                    };
                    _allPidData.Add(_currentPidData);

                    Helpers.ConsolePrint(MinerTag(), "Starting miner " + ProcessTag() + " " + LastCommandLine);

                    StartCoolDownTimerChecker();
                }
                else
                {
                    Helpers.ConsolePrint(MinerTag(), "NOT STARTED " + ProcessTag() + " " + LastCommandLine);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " _Start: " + ex.Message);
            }

            return null;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            if (IsRunning)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Shutting down miner");
            }

            if (_process == null) return;

            try
            {
                _process.Kill();
            }
            catch { }

            _process.Close();
            _process = null;
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
                var data = await _httpClient.GetStringAsync($"http://127.0.0.1:{ApiPort}/api/v1/status/solver");
                api.Speed = ParseApi(data);
                CurrentMinerReadStatus =
                    api.Speed <= 0 ? MinerApiReadStatus.READ_SPEED_ZERO : MinerApiReadStatus.GOT_READ;
                return api;
            }
            catch (Exception e)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
                Helpers.ConsolePrint(MinerTag(), e.Message);
            }

            return api;
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
