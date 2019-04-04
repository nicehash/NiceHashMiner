using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MinerPlugin;
using MinerPlugin.Toolkit;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using static MinerPlugin.Toolkit.MinersApiPortsManager;
using System.Globalization;

namespace CCMinerBase
{
    public abstract class CCMinerBase : MinerBase
    {
        // ccminer can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;
        // command line parts
        private string _devices;
        private string _extraLaunchParameters = "";
        private int _apiPort;
        // lazy init
        //private HttpClient _httpClient = null; // throws exceptions
        private ApiDataHelper apiReader = new ApiDataHelper(); // consider replacing with HttpClient

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.NeoScrypt: return "neoscrypt";
                //case AlgorithmType.Lyra2REv2_UNUSED: return "lyra2v2";
                case AlgorithmType.Decred: return "decred";
                //case AlgorithmType.Lbry_UNUSED: return "lbry";
                //case AlgorithmType.X11Gost_UNUSED: return "sib";
                case AlgorithmType.Blake2s: return "blake2s";
                //case AlgorithmType.Sia_UNUSED: return "sia";
                case AlgorithmType.Keccak: return "keccak";
                case AlgorithmType.Skunk: return "skunk";
                //case AlgorithmType.Lyra2z_UNUSED: return "lyra2z";
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.Lyra2REv3: return "lyra2v3";
            }
            // TODO throw exception
            return "";
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var summaryApiResult = await apiReader.GetApiDataAsync(_apiPort, ApiDataHelper.GetHttpRequestNhmAgentStrin("summary"));
            double totalSpeed = 0;
            int totalPower = 0;
            if (!string.IsNullOrEmpty(summaryApiResult)) {
                // TODO return empty
                try
                {
                    var summaryOptvals = summaryApiResult.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var optvalPairs in summaryOptvals)
                    {
                        var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pair.Length != 2) continue;
                        if (pair[0] == "KHS")
                        {
                            totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                        }   
                    }
                }
                catch
                {}
            }
            // TODO if have multiple GPUs call the threads as well, but maybe not as often since it might crash the miner
            //var threadsApiResult = await _httpClient.GetStringAsync($"{localhost}/threads");
            var threadsApiResult = await apiReader.GetApiDataAsync(_apiPort, ApiDataHelper.GetHttpRequestNhmAgentStrin("threads"));
            var perDeviceSpeedInfo = new List<(string uuid, IReadOnlyList<(AlgorithmType, double)>)>();
            var perDevicePowerInfo = new List<(string, int)>();
            if (!string.IsNullOrEmpty(threadsApiResult))
            {
                // TODO return empty
                try
                {
                    var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var gpu in gpus)
                    {
                        var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var gpuData = (id: -1, power: -1, speed: -1d);
                        foreach (var optvalPairs in gpuOptvalPairs)
                        {
                            var optval = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (optval.Length != 2) continue;
                            if (optval[0] == "GPU")
                            {
                                gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "POWER")
                            {
                                gpuData.power = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "KHS")
                            {
                                gpuData.speed = double.Parse(optval[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            }
                        }
                        // TODO do stuff with it gpuData
                        var device = _miningPairs.Where(kvp => kvp.device.ID == gpuData.id).Select(kvp => kvp.device).FirstOrDefault();
                        if (device != null)
                        {
                            perDeviceSpeedInfo.Add((device.UUID, new List<(AlgorithmType, double)>() { (_algorithmType, gpuData.speed) }));
                            perDevicePowerInfo.Add((device.UUID, gpuData.power));
                            totalPower += gpuData.power;
                        } 
                        
                    }
                }
                catch
                { }
            }
            var ad = new ApiData();
            var total = new List<(AlgorithmType, double)>();
            total.Add((_algorithmType, totalSpeed));
            ad.AlgorithmSpeedsTotal = total;
            ad.PowerUsageTotal = totalPower;
            ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ad.PowerUsagePerDevice = perDevicePowerInfo;

            return ad;
        }

        public override async Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
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

            var algo = AlgorithmName(_algorithmType);
            
            var commandLine = $"--algo={algo} --benchmark --time-limit {benchmarkTime} {_devices} {_extraLaunchParameters}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            // TODO implement fallback average, final benchmark 
            bp.CheckData = (string data) => {
                return MinerToolkit.TryGetHashrateAfter(data, "Benchmark:"); // TODO add option to read totals
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        //// TODO this PATH IS temporary FIXED
        //protected override Tuple<string, string> GetBinAndCwdPaths()
        //{
        //    var binPath = @"D:\Programming\NiceHashMinerLegacy\Release\bin\ccminer_tpruvot\ccminer.exe";
        //    var binCwd = @"D:\Programming\NiceHashMinerLegacy\Release\bin\ccminer_tpruvot\";
        //    return Tuple.Create(binPath, binCwd);
        //}

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
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
            // TODO _miningPairs must not be null or count 0
            //if (_miningPairs == null)
            //throw new NotImplementedException();

            // API port function might be blocking
            _apiPort = GetAvaliablePortInRange(); // use the default range
            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --url={url} --user={_username} --api-bind={_apiPort} {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
