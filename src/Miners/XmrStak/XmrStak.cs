using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;
using System.IO;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;
using XmrStak.Configs;

namespace XmrStak
{
    public class XmrStak : MinerBase
    {
        //private string _devices;
        //protected _cpuDevices = new 
        protected HashSet<DeviceType> _miningDeviceTypes = null;
        protected Dictionary<int, string> _threadsForDeviceUUIDs = null;
        protected string _extraLaunchParameters = "";
        protected int _apiPort;
        protected readonly string _uuid;
        protected AlgorithmType _algorithmType;
        protected readonly HttpClient _http = new HttpClient();
        protected IXmrStakConfigHandler _configHandler;

        public XmrStak(string uuid, IXmrStakConfigHandler configHandler)
        {
            _uuid = uuid;
            _configHandler = configHandler;
        }

        protected override Dictionary<string, string> GetEnvironmentVariables()
        {
            if (MinerSystemEnvironmentVariables != null)
            {
                return MinerSystemEnvironmentVariables.DefaultSystemEnvironmentVariables;
            }
            return null;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.CryptoNightHeavy: return "cryptonight_heavy";
                case AlgorithmType.CryptoNightV8: return "cryptonight_v8";
                case AlgorithmType.CryptoNightR: return "cryptonight_r";
                default: return "";
            }
        }

        protected virtual double DevFee => 0d;

        // call to map device ids parse html first
        private async Task MapMinerDevicesStatsDataAsync()
        {
            try
            {
                _threadsForDeviceUUIDs = new Dictionary<int, string>();
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/h");
                var threadsHashrate = TableParser.ParseTable(result);
                foreach (var thread in threadsHashrate)
                {
                    var deviceType = TableParser.GetDeviceTypeFromInfo(thread);
                    var deviceConfigId = TableParser.GetDeviceConfigIdFromInfo(thread);
                    var threadId = TableParser.GetThreadIdFromInfo(thread);
                    //var device = _miningPairs
                    //    .Where(d => d.Device.DeviceType == deviceType && d.Device.ID == deviceId)
                    //    .Select(d => d.Device)
                    //    .FirstOrDefault();
                    //if (device == null)
                    //{
                    //    // LOG ERROR
                    //    continue;
                    //}
                    //_threadsForDeviceUUIDs[threadId] = device.UUID;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"MapMinerDevicesStatsDataAsync exception: {e}");
            }
        }

        // XmrStak doesn't report power usage
        // TODO map threads
        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            if (_threadsForDeviceUUIDs == null)
            {
                await MapMinerDevicesStatsDataAsync();
            }
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/api.json");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                //var gpus = _miningPairs.Select(pair => pair.Device);
                var totalSpeed = 0d;
                var perDeviceSpeedSum = new Dictionary<string, double>();
                // init per device sums
                foreach (var pair in _miningPairs)
                {
                    perDeviceSpeedSum[pair.Device.UUID] = 0d;
                }

                for (int threadIndex = 0; threadIndex < summary.hashrate.threads.Count; threadIndex++)
                {
                    var thread = summary.hashrate.threads[threadIndex];
                    var currentSpeed = thread.FirstOrDefault() ?? 0d;

                    totalSpeed += currentSpeed;
                    var currentUUID = _threadsForDeviceUUIDs?[threadIndex] ?? "";
                    if (string.IsNullOrEmpty(currentUUID)) continue;
                    perDeviceSpeedSum[currentUUID] += currentSpeed;
                }

                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();

                foreach (var kvp in perDeviceSpeedSum)
                {
                    var UUID = kvp.Key;
                    var currentSpeed = kvp.Value;
                    perDeviceSpeedInfo.Add(UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentSpeed) });
                    // no power usage info
                    perDevicePowerInfo.Add(UUID, -1);
                }

                api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed) };
                api.PowerUsageTotal = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: {e}");
            }

            return api;
        }

        // TODO broken
        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            return new BenchmarkResult();
        }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "xmr-stak.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");
            // all good continue on

            _miningDeviceTypes = new HashSet<DeviceType>(_miningPairs.Select(pair => pair.Device.DeviceType));

            // init command line params parts
            var orderedMiningPairs = _miningPairs.ToList();

            // here extra launch parameters 
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID) - a.Device.DeviceType.CompareTo(b.Device.DeviceType));
            if (MinerOptionsPackage != null)
            {
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange(); // use the default range
            // instant non blocking
            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);

            var algo = AlgorithmName(_algorithmType);
            // prepare configs
            var folder = _algorithmType.ToString().ToLower();

            var deviceConfigParams = $@"--cpu {folder}\cpu.txt";
            // TODO prepare config files
            //if (_miningDeviceTypes.Contains(DeviceType.))
            //{
            //}
            CreateConfigFile(DeviceType.CPU).Wait();
            CreateConfigFile(DeviceType.NVIDIA).Wait();

            var disableDeviceTypes = CommandLineHelpers.DisableDevCmd(_miningDeviceTypes);
            var commandLine = $@"--config {folder}\config.txt --poolconf {folder}\pools.txt {deviceConfigParams} {disableDeviceTypes}";
            return commandLine;
        }

        protected Task<bool> CreateConfigFile(DeviceType deviceType)
        {
            // API port function might be blocking
            var apiPort = MinersApiPortsManager.GetAvaliablePortInRange(); // use the default range
            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);

            var disableDeviceTypes = CommandLineHelpers.DisableDevCmd(new List<DeviceType> { deviceType });
            var currency = AlgorithmName(_algorithmType);
            var commandLine = $"-o {url} -u {MinerToolkit.DemoUser} --currency {currency} -i {apiPort} --use-nicehash -p x -r x --benchmark 10 --benchwork 25 --benchwait 5 {disableDeviceTypes}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;

            var config = $"{deviceType.ToString()}.txt".ToLower();
            return ConfigHelpers.CreateConfigFile(config, binPath, binCwd, commandLine, GetEnvironmentVariables());
        }
    }
}
