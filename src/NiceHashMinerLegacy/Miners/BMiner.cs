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
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner.Miners
{
    public class BMiner : Miner
    {
        #region JSON Models

        private interface ISpeedInfo
        {
            double Hashrate { get; }
        }

        private class EquiSpeedInfo : ISpeedInfo
        {
            public double nonce_rate { get; set; }
            public double solution_rate { get; set; }

            public double Hashrate => solution_rate;
        }

        private class GenericSpeedInfo : ISpeedInfo
        {
            public double hash_rate { get; set; }

            public double Hashrate => hash_rate;
        }

        private class JsonModel<T> where T : ISpeedInfo
        {
            public class Solver
            {
                public string algorithm { get; set; }
                public T speed_info { get; set; }
            }

            public List<Solver> solvers { get; set; }
        }

        #endregion

        private readonly HttpClient _httpClient;

        private Process _process;

        private double _benchHashes = 0;
        private int _benchIters = 0;
        private int _targetBenchIters;

        private bool IsEquihash
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.Equihash:
                    case AlgorithmType.Beam:
                    case AlgorithmType.ZHash:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private double DevFee
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.Equihash:
                    case AlgorithmType.Beam:
                    case AlgorithmType.ZHash:
                        return 2;
                    case AlgorithmType.DaggerHashimoto:
                        return 0.65;
                    case AlgorithmType.Grin:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        private bool IsDual => MiningSetup.CurrentSecondaryAlgorithmType != AlgorithmType.NONE;

        public BMiner(AlgorithmType algo) : base("bminer")
        {
            ConectionType = algo == AlgorithmType.Beam ?
                NhmConectionType.SSL : NhmConectionType.NONE;

            _httpClient = new HttpClient();
        }

        private static string GetScheme(AlgorithmType algo)
        {
            switch (algo)
            {
                case AlgorithmType.Beam:
                    return "beam+ssl";
                case AlgorithmType.Equihash:
                    return "stratum";
                case AlgorithmType.ZHash:
                    return "equihash1445";
                case AlgorithmType.DaggerHashimoto:
                    return "ethstratum";
                case AlgorithmType.Decred:
                    return "blake14r";
                case AlgorithmType.Blake2s:
                    return "blake2s";
                case AlgorithmType.Grin:
                    return "cuckaroo29";
                default:
                    return null;
            }
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

            var scheme = GetScheme(MiningSetup.CurrentAlgorithmType);

            var cmd = $"-uri {scheme}://{user}@{url} -api 127.0.0.1:{ApiPort} " +
                      $"-devices {devs} -watchdog=false ";

            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.ZHash)
            {
                cmd += "-pers auto ";
            }

            if (IsDual)
            {
                var secondUrl = GetServiceUrl(MiningSetup.CurrentSecondaryAlgorithmType);
                var secondScheme = GetScheme(MiningSetup.CurrentSecondaryAlgorithmType);

                cmd += $"-uri2 {secondScheme}://{user}@{secondUrl} ";
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

            Ethlargement.CheckAndStart(MiningSetup);

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
            if (!outdata.TryGetHashrateAfter("Total ", out var hashrate) ||
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
                ISpeedInfo speedInfo;

                if (IsEquihash)
                {
                    speedInfo = dev.ToObject<JsonModel<EquiSpeedInfo>>().solvers[0].speed_info;
                }
                else
                {
                    speedInfo = dev.ToObject<JsonModel<GenericSpeedInfo>>().solvers[0].speed_info;
                }

                hashrate += speedInfo.Hashrate;
            }

            return hashrate;
        }
    }
}
