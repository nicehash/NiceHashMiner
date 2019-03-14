using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPlugin.Toolkit;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Enums;
using static MinerPlugin.Toolkit.MinersApiPortsManager;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;

namespace LolMinerBeam
{
    public class LolMinerBeam : MinerBase
    {
        private string _devices;

        private string _extraLaunchParameters = "";

        private int _apiPort;

        private readonly string _uuid;

        private AlgorithmType _algorithmType;

        private readonly HttpClient _http = new HttpClient();

        public LolMinerBeam (string uuid)
        {
            _uuid = uuid;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Beam: return "BEAM";
                default: return "";
            }
        }

        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.Beam: return 1.0;
                    default: return 0;
                }
            }
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                var summary = JsonConvert.DeserializeObject<ApiJsonResponse>(summaryApiResult);

                var gpuDevices = _miningPairs.Select(pair => pair.device);
                var perDeviceSpeedInfo = new List<(string uuid, IReadOnlyList<(AlgorithmType, double)>)>();
                var perDevicePowerInfo = new List<(string, int)>();
                var totalSpeed = summary.Session.Performance_Summary;

                foreach (var gpuDevice in gpuDevices)
                {
                    var currentStats = summary.GPUs.Where(devStats => devStats.Index == gpuDevice.ID).FirstOrDefault(); //todo index == ID ????
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add((gpuDevice.UUID, new List<(AlgorithmType, double)>() { (_algorithmType, currentStats.Performance) }));
                }

                var total = new List<(AlgorithmType, double)>();
                total.Add((_algorithmType, totalSpeed));
                ad.AlgorithmSpeedsTotal = total;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ad;
        }

        public async override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = 20; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 20;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    break;
            }

            var commandLine = $"--coin BEAM --pool beam.eu.nicehash.com --port 3370 --user 38QriJ13vEBWUPhJrEKKZwKdVftV9dtW25.lolMiner --tls 0 --apiport {_apiPort} {_extraLaunchParameters}";
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            //todo have to implement with benchmark flag when update of lolMiner will arrive (currently benchmarking is not possible)
            //var commandLine = $"--benchmark {algo} --longstats {benchmarkTime}";
            bp.CheckData = (string data) =>
            {
                return MinerToolkit.TryGetHashrateAfter(data, "Total:");
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override (string binPath, string binCwd) GetBinAndCwdPaths()
        {
            var binPath = @"C:\legacyBins\lolMinerBeam\lolMiner.exe";
            var binCwd = @"C:\legacyBins\lolMinerBeam";
            return (binPath, binCwd);
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmType, ok) = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");
            // all good continue on

            // init command line params parts
            var deviceIds = MinerToolkit.GetDevicesIDsInOrder(_miningPairs);
            _devices = $"--devices {string.Join(",", deviceIds)}";
            // TODO implement this later
            //_extraLaunchParameters;
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePortInRange(); // use the default range
            // instant non blocking
            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--coin {algo} --pool {url} --port {port} --user {_username} --tls 0 --apiport {_apiPort} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
