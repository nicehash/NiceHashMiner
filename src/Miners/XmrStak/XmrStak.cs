using MinerPlugin;
using MinerPluginToolkitV1;
using NHM.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using static NHM.Common.StratumServiceHelpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using NHM.Common;
using System.Collections.Generic;
using XmrStak.Configs;
using MinerPluginToolkitV1.Configs;

namespace XmrStak
{
    public class XmrStak : MinerBase
    {
        private static Random _rand { get; } = new Random();
        //private string _devices;
        //protected _cpuDevices = new 
        protected HashSet<DeviceType> _miningDeviceTypes = null;
        protected Dictionary<int, string> _threadsForDeviceUUIDs = null;
        protected int _apiPort;
        protected readonly HttpClient _http = new HttpClient();
        protected IXmrStakConfigHandler _configHandler;
        protected CancellationTokenSource _stopSource = null;

        // running configs
        protected CpuConfig _cpuConfig;
        protected AmdConfig _amdConfig;
        protected NvidiaConfig _nvidiaConfig;

        public XmrStak(string uuid, IXmrStakConfigHandler configHandler) : base(uuid)
        {
            _configHandler = configHandler;
        }

        protected void StopSource()
        {
            try
            {
                _stopSource?.Cancel();
            }
            catch (Exception)
            {
            }
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.CryptoNightR: return "cryptonight_r";
                default: return "";
            }
        }

        protected virtual double DevFee => 0d;

        private IEnumerable<int> GetThreadsForDeviceUUID(string uuid)
        {
            if (_threadsForDeviceUUIDs != null)
            {
                foreach (var kvp in _threadsForDeviceUUIDs)
                {
                    var thread = kvp.Key;
                    var compareUuid = kvp.Value;
                    if (uuid == compareUuid)
                    {
                        yield return thread;
                    }
                }
            }
        }

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
                        Logger.Info(_logGroup, $"Device was not found: {deviceType.ToString()}");
                        continue;
                    }
                    _threadsForDeviceUUIDs[threadId] = deviceUUID;
                }
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while mapping devices: {e.Message}");
            }
        }

        // XmrStak doesn't report power usage
        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            if (_threadsForDeviceUUIDs == null || _threadsForDeviceUUIDs.Count == 0)
            {
                await MapMinerDevicesStatsDataAsync();
            }
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/api.json");
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var totalSpeed = 0d;
                var threadsSpeeds = summary.hashrate.threads;
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                // init per device sums
                foreach (var pair in _miningPairs)
                {
                    var UUID = pair.Device.UUID;
                    var threads = GetThreadsForDeviceUUID(UUID);
                    var speedsPerThread = threads
                        .Where(thread => thread < threadsSpeeds.Count)
                        .Select(thread => threadsSpeeds[thread]?.FirstOrDefault() ?? 0d);
                    
                    var currentSpeed = speedsPerThread.Sum();
                    totalSpeed += currentSpeed;
                    perDeviceSpeedInfo.Add(UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentSpeed * (1 - DevFee * 0.01)) });
                    // no power usage info
                    perDevicePowerInfo.Add(UUID, -1);
                }

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                api.PowerUsageTotal = -1;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }

        // TODO account AMD kernel building
        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // END prepare config block

            // determine benchmark time 
            // settup times
            var openCLCodeGenerationWait = _miningDeviceTypes.Contains(DeviceType.AMD) ? 20 : 0;
            var benchWait = 5;
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 30, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds


            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            // this one here might block
            string deviceConfigParams = "";
            try
            {
                deviceConfigParams = await PrepareDeviceConfigs(stop);
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
            var apiPort = GetAvaliablePort();
            var commandLine = $"-o {url} -u {MinerToolkit.DemoUserBTC} --currency {algo} -i {apiPort} --use-nicehash -p x -r x --benchmark 10 --benchwork {benchmarkTime} --benchwait {benchWait} {deviceConfigParams} {disableDeviceTypes}";
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
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

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "xmr-stak.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override void Init()
        {
            _miningDeviceTypes = new HashSet<DeviceType>(_miningPairs.Select(pair => pair.Device.DeviceType));
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binCwd = binPathBinCwdPair.Item2;
            var algo = AlgorithmName(_algorithmType);
            // prepare configs
            var folder = _algorithmType.ToString().ToLower();
            // run in new task so we don't deadlock main thread
            var deviceConfigParams = Task.Run(() => PrepareDeviceConfigs(CancellationToken.None)).Result;
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

        protected async Task<string> PrepareDeviceConfigs(CancellationToken stop)
        {
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binCwd = binPathBinCwdPair.Item2;
            var algo = AlgorithmName(_algorithmType);
            // prepare configs
            var folder = _algorithmType.ToString().ToLower();
            // check if we have configs
            var createConfigs = _miningDeviceTypes.All(deviceType => !_configHandler.HasConfig(deviceType, _algorithmType));
            if (createConfigs)
            {
                using (_stopSource = new CancellationTokenSource())
                using (var linked = CancellationTokenSource.CreateLinkedTokenSource(stop, _stopSource.Token))
                {
                    try
                    {
                        var t = CreateConfigFiles(_miningDeviceTypes, linked.Token);
                        var tupleResult = await t;
                        var result = tupleResult.Item1;
                        var configFilePaths = tupleResult.Item2;
                        if (result)
                        {
                            foreach (var path in configFilePaths)
                            {
                                var deviceTypes = _miningDeviceTypes.Where(deviceType => path.Contains(deviceType.ToString()));
                                if (deviceTypes.Count() > 0)
                                {
                                    var deviceType = deviceTypes.First();
                                    _configHandler.SaveMoveConfig(deviceType, _algorithmType, path);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Info(_logGroup, $"Failed to create config file: {e.Message}");
                    }
                }
            }

            // wait until we have the algorithms ready, 5 seconds should be enough
            foreach (var deviceType in _miningDeviceTypes)
            {
                var hasConfig = false;
                var start = DateTime.UtcNow;
                while (DateTime.UtcNow.Subtract(start).Seconds < 5)
                {
                    await Task.Delay(100);
                    hasConfig = _configHandler.HasConfig(deviceType, _algorithmType);
                    if (hasConfig) break;
                }
                if (!hasConfig)
                {
                    Logger.Info(_logGroup, $"Config for {deviceType.ToString()}_{_algorithmType.ToString()} not found!");
                    throw new Exception($"Cannot start device type {deviceType.ToString()} for algorithm {_algorithmType.ToString()} there is no config");
                } 
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
                    var openClAmdPlatformResult = MinerToolkit.GetOpenCLPlatformID(_miningPairs);
                    var openClAmdPlatformNum = openClAmdPlatformResult.Item1;
                    bool openClAmdPlatformNumUnique = openClAmdPlatformResult.Item2;
                    if (!openClAmdPlatformNumUnique)
                    {
                        Logger.Error(_logGroup, "Initialization of miner failed. Multiple OpenCLPlatform IDs found!");
                        throw new InvalidOperationException("Invalid mining initialization");
                    }
                    var amdTemplate = _configHandler.GetAmdConfig(_algorithmType);
                    _amdConfig = new AmdConfig() { platform_index = openClAmdPlatformNum };
                    _amdConfig.gpu_threads_conf = amdTemplate.gpu_threads_conf.Where(t => deviceIDs.Contains(t.index)).ToList();
                    ConfigHelpers.WriteConfigFile(deviceConfigFilePath, _amdConfig);
                }
            }
            return deviceConfigParams;
        }

        // TODO add cancel token
        protected async Task<Tuple<bool, IEnumerable<string>>> CreateConfigFiles(IEnumerable<DeviceType> deviceTypes, CancellationToken stop)
        {
            //var tag = string.Join("_", _miningPairs.Select(pair => $"{pair.Device.DeviceType.ToString()}_{pair.Device.ID}"));
            //var genPrefix = $"gen_{_algorithmType.ToString()}_{tag}";
            var genPrefix = $"gen_{_algorithmType.ToString()}_{_rand.Next().ToString()}";
            var deviceTypeIDs = _miningPairs.Select(pair => Tuple.Create(pair.Device.DeviceType, pair.Device.ID));
            var enableDeviceFlagsConfigFiles = CommandLineHelpers.GetConfigCmd(genPrefix, deviceTypeIDs);
            var enableDeviceTypesStr = string.Join(" ", enableDeviceFlagsConfigFiles.Select(pair => $"{pair.Item1} {pair.Item2}"));

            var configFlagAndFiles = CommandLineHelpers.GetGeneralAndPoolsConf(genPrefix);
            var configFlagAndFilesStr = string.Join(" ", configFlagAndFiles);

            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var disableDeviceTypes = CommandLineHelpers.DisableDevCmd(deviceTypes);
            var currency = AlgorithmName(_algorithmType);
            var commandLine = $"-o {url} -u {MinerToolkit.DemoUserBTC} --currency {currency} -i 0 --use-nicehash -p x -r x --benchmark 10 --benchwork 60 --benchwait 5 {disableDeviceTypes} {enableDeviceTypesStr} {configFlagAndFilesStr}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            var envVars = GetEnvironmentVariables();
            var configs = enableDeviceFlagsConfigFiles.Select(p => p.Item2);
            var success = await ConfigHelpers.CreateConfigFiles(configs, binPath, binCwd, commandLine, envVars, stop);
            var configsFullPath = configs.Select(path => Path.Combine(binCwd, path));

            var deleteFiles = Directory.GetFiles(binCwd, "*.*", SearchOption.TopDirectoryOnly).Where(p => p.Contains(genPrefix) && p.Contains("conf"));
            foreach (var delete in deleteFiles)
            {
                try
                {
                    File.Delete(Path.Combine(binCwd, delete));
                }
                catch (Exception)
                {}
            }

            return Tuple.Create(success, configsFullPath);
        }

        public override void StopMining()
        {
            StopSource();
            base.StopMining();
        }
    }
}
