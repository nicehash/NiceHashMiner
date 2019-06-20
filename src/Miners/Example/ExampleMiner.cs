using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    /// <summary>
    /// Miner class is used to define behaviour of some basic functions like Benchmarking, getting data from miner API, setting command line, etc.
    /// Miner class inherits MinerBase for the pallete of miner action functions like Start/Stop mining, Start benchmarking, get data from API, create command line, etc.
    /// </summary>
    public class ExampleMiner : MinerBase
    {
        /////////// variable region ///////////

        private HttpClient _httpClient;
        private int _apiPort;

        // GMiner can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;

        // command line parts
        private string _extraLaunchParameters = "";
        private string _devices;

        /////////// end of variable region ///////////

        /// <summary>
        /// Constructor to create a miner with uuid
        /// </summary>
        public ExampleMiner(string uuid) : base(uuid) { }

        /// <summary>
        /// AlgorithmName returns algorithm name used in command line
        /// </summary>
        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Beam: return "beam";
                case AlgorithmType.GrinCuckaroo29: return "cuckaroo29";
                case AlgorithmType.GrinCuckatoo31: return "cuckatoo31";
                case AlgorithmType.CuckooCycle: return "aeternity";
                default: return "";
            }
        }

        /// <summary>
        /// Sets the developer fee in percentage for calculations in benchmarking and api data
        /// </summary>
        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.CuckooCycle: return 1.5;
                    case AlgorithmType.Beam:
                    case AlgorithmType.GrinCuckaroo29:
                    case AlgorithmType.GrinCuckatoo31:
                    default: return 2.0;
                }
            }
        }

        /// <summary>
        /// GetBinAndCwdPaths is used to define bin and root path
        /// </summary>
        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "exampleMiner.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        /// <summary>
        /// GetMinerStatsDataAsync implements behaviour to get data from miner API
        /// </summary>
        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            if (_httpClient == null) _httpClient = new HttpClient();
            var ad = new ApiData();
            try
            {
                // get data from api webpage / some miners require GET request
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/stat");
                // deserialize json object into internal class prepared for json api response
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                // set data to ApiData object by bisecting returned miner API data
                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                // go through all devices and find data from api, that belongs to that device
                foreach (var gpu in gpus)
                {
                    if (summary.miners == null) continue;

                    var currentSpeed = summary.miners[$"{gpu.ID}"].solver.solution_rate;
                    totalSpeed += currentSpeed;
                    perDeviceSpeedInfo.Add(gpu.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentSpeed * (1 - DevFee * 0.01)) }); // don't forget to substract Developer Fee from speeds
                    var currentPower = summary.miners[$"{gpu.ID}"].device.power;
                    totalPowerUsage += currentPower;
                    perDevicePowerInfo.Add(gpu.UUID, currentPower);
                }
                
                // fill api data
                ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) }; // don't forget to substract Developer Fee from speeds
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }

        /// <summary>
        /// StartBenchmark starts benchmark process and awaits its results
        /// </summary>
        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            var benchmarkTime = 30; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 30;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    break;
            }

            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"-uri {algo}://{_username}@{url}:{port} {_devices} -watchdog=false {_extraLaunchParameters}";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, "Total");
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;

                if (found)
                {
                    benchHashes += hashrate;
                    benchIters++;

                    benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01); // don't forget to substract developer fee
                }

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = benchIters >= targetBenchIters
                };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }
        
        /// <summary>
        /// Initialize all extra stuff for creation of command line 
        /// </summary>
        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Info(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }
            // all good continue on

            // init command line params parts
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            var deviceIDs = _miningPairs.Select(p =>
            {
                var device = p.Device;
                var prefix = device.DeviceType == DeviceType.AMD ? "amd:" : "";
                return prefix + device.ID;
            }).OrderBy(id => id);
            _devices = $"-devices {string.Join(",", deviceIDs)}";

            if (MinerOptionsPackage != null)
            {
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        /// <summary>
        /// Creates command line for usage in <see cref="StartBenchmark"/> and <see cref="MinerBase.StartMining"/> functions
        /// </summary>
        protected override string MiningCreateCommandLine()
        {
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];

            var algo = AlgorithmName(_algorithmType);
            var commandLine = $"-uri {algo}://{_username}@{url}:{port} -api 127.0.0.1:{_apiPort} {_devices} -watchdog=false {_extraLaunchParameters}"; // don't forget to disable miner watchdog otherwise there is a danger for duplicated mining windows
            return commandLine;
        }
    }
}
