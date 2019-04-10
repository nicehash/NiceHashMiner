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

        // running configs
        protected CpuConfig _cpuConfig;
        protected AmdConfig _amdConfig;
        protected NvidiaConfig _nvidiaConfig;

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

                    var deviceUUID = "";
                    if (deviceType == DeviceType.CPU)
                    {
                        var cpu = _miningPairs
                            .Where(d => d.Device.DeviceType == deviceType)
                            .Select(d => d.Device)
                            .FirstOrDefault();
                        if (cpu != null)
                        {
                            deviceUUID = cpu.UUID;
                        }
                    }
                    if (deviceType == DeviceType.AMD)
                    {
                        var openCL_ID = _amdConfig.gpu_threads_conf[deviceConfigId].index;
                        var amd = _miningPairs
                            .Where(d => d.Device.DeviceType == deviceType && openCL_ID == d.Device.ID)
                            .Select(d => d.Device)
                            .FirstOrDefault();
                        if (amd != null)
                        {
                            deviceUUID = amd.UUID;
                        }
                    }
                    if (deviceType == DeviceType.NVIDIA)
                    {
                        var cudaID = _nvidiaConfig.gpu_threads_conf[deviceConfigId].index;
                        var nvidia = _miningPairs
                            .Where(d => d.Device.DeviceType == deviceType && cudaID == d.Device.ID)
                            .Select(d => d.Device)
                            .FirstOrDefault();
                        if (nvidia != null)
                        {
                            deviceUUID = nvidia.UUID;
                        }
                    }
                    if (string.IsNullOrEmpty(deviceUUID))
                    {
                        // TODO log
                        continue;
                    }
                    _threadsForDeviceUUIDs[threadId] = deviceUUID;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"MapMinerDevicesStatsDataAsync exception: {e}");
            }
        }

        // XmrStak doesn't report power usage
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

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed) };
                api.PowerUsageTotal = -1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: {e}");
            }

            return api;
        }

        // TODO account AMD kernel building
        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // END prepare config block

            // determine benchmark time 
            // settup times
            var openCLCodeGenerationWait = 0;
            var benchWait = 5;
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

            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            // this one here might block
            string deviceConfigParams = "";
            try
            {
                deviceConfigParams = await PrepareDeviceConfigs();
            }
            catch (Exception e)
            {
                return new BenchmarkResult
                {
                    ErrorMessage = e.Message
                };
            }
            
            var disableDeviceTypes = CommandLineHelpers.DisableDevCmd(_miningDeviceTypes);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            // API port function might be blocking
            var apiPort = MinersApiPortsManager.GetAvaliablePortInRange(); // use the default range
            var commandLine = $"-o {url} -u {MinerToolkit.DemoUser} --currency {algo} -i {apiPort} --use-nicehash -p x -r x --benchmark 10 --benchwork {benchmarkTime} --benchwait {benchWait} {deviceConfigParams} {disableDeviceTypes}";
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, "Benchmark Total:");
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;

                if (found)
                {
                    benchHashes += hashrate;
                    benchIters++;

                    benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);
                }

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = found
                };
            };

            // always add 10second extra
            var benchmarkTimeout = TimeSpan.FromSeconds(10 + benchmarkTime + benchWait + openCLCodeGenerationWait);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
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

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binCwd = binPathBinCwdPair.Item2;
            var algo = AlgorithmName(_algorithmType);
            // prepare configs
            var folder = _algorithmType.ToString().ToLower();

            var blockingTask = PrepareDeviceConfigs();
            blockingTask.Wait();
            var deviceConfigParams = blockingTask.Result;
            // prepare pools and config
            var generalConfigFilePath = Path.Combine(binCwd, folder, "config.txt");
            var generalConfig = new MainConfig{ httpd_port = _apiPort };
            ConfigHelpers.WriteConfigFile(generalConfigFilePath, generalConfig);
            var poolsConfigFilePath = Path.Combine(binCwd, folder, "pools.txt");
            var poolsConfig = new PoolsConfig(urlWithPort, _username, algo);
            ConfigHelpers.WriteConfigFile(poolsConfigFilePath, poolsConfig);

            var disableDeviceTypes = CommandLineHelpers.DisableDevCmd(_miningDeviceTypes);
            var commandLine = $@"--config {folder}\config.txt --poolconf {folder}\pools.txt {deviceConfigParams} {disableDeviceTypes}";
            return commandLine;
        }

        protected async Task<string> PrepareDeviceConfigs()
        {
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binCwd = binPathBinCwdPair.Item2;
            var algo = AlgorithmName(_algorithmType);
            // prepare configs
            var folder = _algorithmType.ToString().ToLower();
            // check if we have configs
            foreach (var deviceType in _miningDeviceTypes)
            {
                var flag = deviceType.ToString().ToLower();
                var config = $"{flag}.txt";
                var configFilePath = Path.Combine(binCwd, config);
                if (!_configHandler.HasConfig(deviceType, _algorithmType))
                {
                    try
                    {
                        var t = CreateConfigFile(deviceType);
                        var result = await t; 
                        if (result)
                        {
                            _configHandler.SaveMoveConfig(deviceType, _algorithmType, configFilePath);
                        }
                    }
                    catch (Exception)
                    {
                        // TODO log
                    }
                }
            }
            // wait until we have the algorithms ready, 5 seconds should be enough
            foreach (var deviceType in _miningDeviceTypes)
            {
                var hasConfig = false;
                var start = DateTime.Now;
                while (DateTime.Now.Subtract(start).Seconds < 5)
                {
                    await Task.Delay(100);
                    hasConfig = _configHandler.HasConfig(deviceType, _algorithmType);
                    if (hasConfig) break;
                }
                if (!hasConfig) throw new Exception($"Cannot start device type {deviceType.ToString()} for algorithm {_algorithmType.ToString()} there is no config");
            }


            var deviceConfigParams = "";
            // prepare
            foreach (var deviceType in _miningDeviceTypes)
            {
                var flag = deviceType.ToString().ToLower();
                // for filtering devices by CUDA and OpenCL indexes
                var deviceIDs = _miningPairs.Where(pair => pair.Device.DeviceType == deviceType).Select(pair => pair.Device.ID);
                var config = $"{flag}_{string.Join(",", deviceIDs)}.txt".ToLower();
                
                deviceConfigParams += $@" --{flag} {folder}\{config}";
                var deviceConfigFilePath = Path.Combine(binCwd, folder, config);

                
                if (_cpuConfig == null && DeviceType.CPU == deviceType)
                {
                    // we just use the template
                    _cpuConfig = _configHandler.GetCpuConfig(_algorithmType);
                    ConfigHelpers.WriteConfigFile(deviceConfigFilePath, _cpuConfig);
                }
                if (_nvidiaConfig == null && DeviceType.NVIDIA == deviceType)
                {
                    var nvidiaTemplate = _configHandler.GetNvidiaConfig(_algorithmType);
                    _nvidiaConfig = new NvidiaConfig();
                    _nvidiaConfig.gpu_threads_conf = nvidiaTemplate.gpu_threads_conf.Where(t => deviceIDs.Contains(t.index)).ToList();
                    ConfigHelpers.WriteConfigFile(deviceConfigFilePath, _nvidiaConfig);
                }
                if (_amdConfig == null && DeviceType.AMD == deviceType)
                {
                    var amdTemplate = _configHandler.GetAmdConfig(_algorithmType);
                    _amdConfig = new AmdConfig();
                    _amdConfig.gpu_threads_conf = amdTemplate.gpu_threads_conf.Where(t => deviceIDs.Contains(t.index)).ToList();
                    ConfigHelpers.WriteConfigFile(deviceConfigFilePath, _amdConfig);
                }
            }
            return deviceConfigParams;
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
