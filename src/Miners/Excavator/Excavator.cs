using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Excavator
{
    public class Excavator : MinerBase, IAfterStartMining
    {
        private int _apiPort;

        public Excavator(string uuid) : base(uuid)
        { }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        private object _lockLastApiData = new object();
        private ApiData _lastApiData = null;
        private HttpClient _httpClient;
        private const bool USE_HTTP_CLIENT = true;
        private string _authToken = Guid.NewGuid().ToString();

        private ApiData LastApiData
        {
            get
            {
                lock (_lockLastApiData)
                {
                    var ad = new ApiData();
                    if (_lastApiData != null)
                    {
                        ad.PowerUsageTotal = _lastApiData.PowerUsageTotal;
                        ad.AlgorithmSpeedsPerDevice = _lastApiData.AlgorithmSpeedsPerDevice;
                        ad.PowerUsagePerDevice = _lastApiData.PowerUsagePerDevice;
                        ad.ApiResponse = _lastApiData.ApiResponse;
                    }
                    return ad;
                }
            }
            set
            {
                lock (_lockLastApiData)
                {
                    _lastApiData = value;
                }
            }
        }

        public async Task<string> HttpGet(string command)
        {
            try
            {
                // lazy init
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", _authToken);
                }
                //Authorization
                var requestUri = Uri.EscapeUriString($"http://localhost:{_apiPort}/api?command={command}");
                // http://bind-ip:bind-port/api?command=%7BJSON-command-here%7D
                Logger.Info(_logGroup, $"Excavator_DELETE HttpGet requestUri: '{requestUri}'");
                var result = await _httpClient.GetStringAsync(requestUri);
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Excavator HttpGet error: {e.Message}");
                return "";
            }
        }

        public static async Task<string> GetApiDataAsync(int port, string dataToSend, string logGroup, CancellationToken stop)
        {
            try
            {
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                using (var ct = CancellationTokenSource.CreateLinkedTokenSource(stop, timeout.Token))
                using (var client = new TcpClient("127.0.0.1", port))
                using (var nwStream = client.GetStream())
                {
                    var bytesToSend = Encoding.ASCII.GetBytes(dataToSend);
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length, ct.Token);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize, ct.Token);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    client.Close();
                    return respStr;
                }
            }
            catch (Exception e)
            {
                Logger.Error(logGroup, $"Excavator GetApiDataAsync error: {e.Message}");
                return "";
            }
        }

        private async Task<string> ExecuteCommand(string command, CancellationToken stop)
        {
            try
            {
                if (USE_HTTP_CLIENT)
                {
                    _ = stop;
                    var response = await HttpGet(command);
                    return response;
                }
                else
                {
                    var response = await GetApiDataAsync(_apiPort, command + "\r\n", _logGroup, stop);
                    return response;
                }
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured with command '{command}' error: {e.Message}");
            }
            return null;
        }

        public override async Task<ApiData> GetMinerStatsDataAsync()
        {
            await Task.CompletedTask; // stub just to have same interface 
            return LastApiData; // return last read speed
        }

        private async Task<ApiData> GetMinerStatsDataAsyncPrivate(CancellationToken stop)
        {
            var ad = new ApiData();
            try
            {
                const string speeds = @"{""id"":123456789,""method"":""worker.list"",""params"":[]}";
                var response = await ExecuteCommand(speeds, stop);
                ad.ApiResponse = response;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(response);
                var gpus = _miningPairs.Select(pair => pair.Device.UUID);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                foreach (var gpu in gpus)
                {
                    var speed = summary.workers.Where(w => w.device_uuid == gpu).SelectMany(w => w.algorithms.Select(a => a.speed)).Sum();
                    perDeviceSpeedInfo.Add(gpu, new List<(AlgorithmType type, double speed)>() { (_algorithmType, speed) });
                }
                ad.PowerUsageTotal = 0;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Logger.Error("EXCAVATOR-API_ERR", e.ToString());
            }
            return ad;
        }

        protected override void Init() { }

        private static string GetServiceLocation(string miningLocation)
        {
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return $"nhmp-ssl-test.{miningLocation}.nicehash.com:443";
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return $"stratum-dev.{miningLocation}.nicehash.com:443";
            //BuildTag.PRODUCTION
            return $"nhmp-ssl.{miningLocation}.nicehash.com:443";
        }

        private static (string templateStr, bool oldTemplate) CmdJSONString(string pluginUUID, string miningLocation, string username, params string[] uuids)
        {
            var templatePath = CmdConfig.CommandFileTemplatePath(pluginUUID);
            var miningServiceLocation = GetServiceLocation(miningLocation);
            var str = CmdConfig.CreateCommandFileWithTemplate(miningServiceLocation, username, uuids, templatePath);
            if (str != null) return (str, false);
            const string DEVICE = @"		{""id"":3,""method"":""worker.add"",""params"":[""daggerhashimoto"",""_DEV_ID_""]}";
            const string TEMPLATE = @"
[
	{""time"":0,""commands"":[
		{""id"":1,""method"":""subscribe"",""params"":[""_MINING_SERVICE_LOCATION_"",""_PUT_YOUR_BTC_HERE_""]}
	]},
	{""time"":1,""commands"":[
        {""id"":1,""method"":""algorithm.add"",""params"":[""daggerhashimoto""]}
    ]},
	{""time"":2,""commands"":[
_DEVICES_
	]}
]";
            var devices = string.Join(",\n", uuids.Select(uuid => DEVICE.Replace("_DEV_ID_", uuid)));
            var oldTemplateStr = TEMPLATE
                .Replace("_MINING_SERVICE_LOCATION_", miningServiceLocation)
                .Replace("_PUT_YOUR_BTC_HERE_", username)
                .Replace("_DEVICES_", devices);
            return (oldTemplateStr, true);
        }

        private static string GetMiningLocation(string location)
        {
            // new mining locations new clients
            if (location.StartsWith("eu") || location.StartsWith("usa")) return location;
            // old mining locations old clients with obsolete locations fallback to usa
            return "usa";
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            var uuids = _miningPairs.Select(p => p.Device).Cast<CUDADevice>().Select(gpu => gpu.UUID);
            var ids = _miningPairs.Select(p => p.Device).Cast<CUDADevice>().Select(gpu => gpu.PCIeBusID);
            //var algo = AlgorithmName(_algorithmType);
            // "--algo {algo} --url={urlWithPort} --user {_username} 
            var (_, cwd) = GetBinAndCwdPaths();
            var fileName = $"cmd_{string.Join("_", ids)}.json";
            //Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            //var logName = $"log_{string.Join("_", ids)}_{unixTimestamp}.log";
            var (cmdStr, isOldCmd) = CmdJSONString(_uuid, GetMiningLocation(_miningLocation), _username, uuids.ToArray());
            if (isOldCmd) Logger.Warn(_logGroup, $"MiningCreateCommandLine usign old command file template");
            File.WriteAllText(Path.Combine(cwd, fileName), cmdStr);
            if (USE_HTTP_CLIENT)
            {
                var commandLine = $"-wp {_apiPort} -wa \"{_authToken}\" -c {fileName} -m -qx {_extraLaunchParameters}";
                return commandLine;
            }
            else
            {
                var commandLine = $"-p {_apiPort} -c {fileName} -m -qx {_extraLaunchParameters}";
                return commandLine;
            }
        }

        void IAfterStartMining.AfterStartMining()
        {
            var ct = new CancellationTokenSource();
            _miningProcess.Exited += (s, e) =>
            {
                try
                {
                    ct?.Cancel();
                }
                catch
                { }
            };
            _ = MinerSpeedsLoop(ct);
        }

        private static bool IsSpeedOk(ApiData ad)
        {
            const double PER_GPU_ANOMALY = 200 * 1000 * 1000; // 200MH/s
            const double SUM_GPU_ANOMALY = 2 * 1000 * 1000 * 1000; // 2GH/s
            if (ad == null) return false; // no speeds
            if (ad.AlgorithmSpeedsPerDevice == null) return false; // no speeds
            var speedsPerDevice = ad.AlgorithmSpeedsPerDevice.Values.Select(speeds => speeds.Select(pair => pair.speed).FirstOrDefault()).ToArray();
            var isPerGPUAnomaly = speedsPerDevice.Any(deviceSpeed => deviceSpeed >= PER_GPU_ANOMALY);
            var isPerGPUZeroSpeed = speedsPerDevice.Any(deviceSpeed => Math.Abs(deviceSpeed) < Double.Epsilon);
            var isSumGPUAnomaly = speedsPerDevice.Sum() >= SUM_GPU_ANOMALY;
            if (isPerGPUAnomaly || isPerGPUZeroSpeed || isSumGPUAnomaly) return false; // speeds anomally

            return true;
        }

        public override async Task StopMiningTask()
        {
            var quit = @"{""id"":123456789,""method"":""quit"",""params"":[]}";
            _ = await ExecuteCommand(quit, CancellationToken.None);
            await base.StopMiningTask();
        }

        private async Task MinerSpeedsLoop(CancellationTokenSource ct)
        {
            Logger.Info("EXCAVATOR-MinerSpeedsLoop", $"STARTING");
            try
            {
                var workers = string.Join(",", _miningPairs.Select((_, i) => $@"""{i}"""));
                var workersReset = @"{""id"":1,""method"":""workers.reset"",""params"":[__WORKERS__]}".Replace("__WORKERS__", workers);
                Func<bool> isActive = () => !ct.Token.IsCancellationRequested;
                var lastSuccessfulSpeeds = DateTime.UtcNow;
                while (isActive())
                {
                    var elapsed = DateTime.UtcNow - lastSuccessfulSpeeds;
                    if (elapsed >= TimeSpan.FromSeconds(50))
                    {
                        Logger.Info("EXCAVATOR-MinerSpeedsLoop", $"Restaring excavator due to speed anomaly");
                        //_ = await ExecuteCommand(@"{""id"":1,""method"":""quit"",""params"":[]}");
#warning if no nhm watchdog it will never restart 'nhms'
                        await StopMiningTask();
                    }
                    try
                    {
                        _ = await ExecuteCommand(workersReset, ct.Token);
                        if (isActive()) await ExcavatorTaskHelpers.TryDelay(TimeSpan.FromSeconds(30), ct.Token);
                        // get speeds
                        var ad = await GetMinerStatsDataAsyncPrivate(ct.Token);
                        LastApiData = ad;
                        if (IsSpeedOk(ad)) lastSuccessfulSpeeds = DateTime.UtcNow;
                        // speed print and reset
                        _ = await ExecuteCommand(@"{""id"":1,""method"":""worker.print.efficiencies"",""params"":[]}", ct.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("EXCAVATOR-MinerSpeedsLoop", $"Error {e}");
                    }
                }
                Logger.Info("EXCAVATOR-MinerSpeedsLoop", $"EXIT WHILE");
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.Error("EXCAVATOR-API_LOOP", $"error {ex}");
            }
            finally
            {
                ct.Dispose();
            }
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            using (var tickCancelSource = new CancellationTokenSource())
            {
                var workers = string.Join(",", _miningPairs.Select((_, i) => $@"""{i}"""));
                var workersReset = @"{""id"":1,""method"":"" workers.reset"",""params"":[__WORKERS__]}".Replace("__WORKERS__", workers);

                // determine benchmark time 
                // settup times
                var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 20, 40, 60 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds
                var maxTicks = MinerBenchmarkTimeSettings.ParseBenchmarkTicks(new List<int> { 1, 3, 9 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType);
                var maxTicksEnabled = MinerBenchmarkTimeSettings.MaxTicksEnabled;

                //// use demo user and disable the watchdog
                var commandLine = MiningCreateCommandLine();
                var (binPath, binCwd) = GetBinAndCwdPaths();
                Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
                Logger.Info(_logGroup, $"Benchmarking settings: time={benchmarkTime} ticks={maxTicks} ticksEnabled={maxTicksEnabled}");
                var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());
                // disable line readings and read speeds from API
                bp.CheckData = null;

                var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
                var benchmarkWait = TimeSpan.FromMilliseconds(500);
                var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop, tickCancelSource.Token);


                var stoppedAfterTicks = false;
                var validTicks = 0;
                var ticks = benchmarkTime / 10; // on each 10 seconds tick
                var result = new BenchmarkResult();
                var benchmarkApiData = new List<ApiData>();
                for (var tick = 0; tick < ticks; tick++)
                {
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;
                    _ = await ExecuteCommand(workersReset, stop);
                    await ExcavatorTaskHelpers.TryDelay(TimeSpan.FromSeconds(10), stop);
                    if (t.IsCompleted || t.IsCanceled || stop.IsCancellationRequested) break;

                    // get speeds
                    var ad = await GetMinerStatsDataAsyncPrivate(stop);
                    var adTotal = ad.AlgorithmSpeedsTotal();
                    var isTickValid = adTotal.Count > 0 && adTotal.All(pair => pair.speed > 0);
                    benchmarkApiData.Add(ad);
                    if (isTickValid) ++validTicks;
                    if (maxTicksEnabled && validTicks >= maxTicks)
                    {
                        stoppedAfterTicks = true;
                        break;
                    }
                }
                // await benchmark task
                if (stoppedAfterTicks)
                {
                    try
                    {
                        tickCancelSource.Cancel();
                    }
                    catch
                    { }
                }
                await t;
                if (stop.IsCancellationRequested)
                {
                    return t.Result;
                }

                // calc speeds
                // TODO calc std deviaton to reduce invalid benches
                try
                {
                    var nonZeroSpeeds = benchmarkApiData.Where(ad => ad.AlgorithmSpeedsTotal().Count > 0 && ad.AlgorithmSpeedsTotal().All(pair => pair.speed > 0))
                                                        .Select(ad => (ad, ad.AlgorithmSpeedsTotal().Count)).ToList();
                    var speedsFromTotals = new List<(AlgorithmType type, double speed)>();
                    if (nonZeroSpeeds.Count > 0)
                    {
                        var maxAlgoPiarsCount = nonZeroSpeeds.Select(adCount => adCount.Count).Max();
                        var sameCountApiDatas = nonZeroSpeeds.Where(adCount => adCount.Count == maxAlgoPiarsCount).Select(adCount => adCount.ad).ToList();
                        var firstPair = sameCountApiDatas.FirstOrDefault();
                        var speedSums = firstPair.AlgorithmSpeedsTotal().Select(pair => new KeyValuePair<AlgorithmType, double>(pair.type, 0.0)).ToDictionary(x => x.Key, x => x.Value);
                        // sum 
                        foreach (var ad in sameCountApiDatas)
                        {
                            foreach (var pair in ad.AlgorithmSpeedsTotal())
                            {
                                speedSums[pair.type] += pair.speed;
                            }
                        }
                        // average
                        foreach (var algoId in speedSums.Keys.ToArray())
                        {
                            speedSums[algoId] /= sameCountApiDatas.Count;
                        }
                        result = new BenchmarkResult
                        {
                            AlgorithmTypeSpeeds = firstPair.AlgorithmSpeedsTotal().Select(pair => (pair.type, speedSums[pair.type])).ToList(),
                            Success = true
                        };
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(_logGroup, $"benchmarking AlgorithmSpeedsTotal error {e.Message}");
                }

                // return API result
                return result;
            }
        }
    }
}
