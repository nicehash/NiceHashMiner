using MinerPlugin;
using MinerPluginToolkitV1;
using Newtonsoft.Json;
using NHM.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static NHM.Common.StratumServiceHelpers;
using System.IO;
using NHM.Common;
using System.Net.Sockets;
using System.Text;
using MinerPluginToolkitV1.Configs;

namespace EWBF
{
    public class EwbfMiner : MinerBase
    {
        // default is 2% and can be changed with --fee parameter
        private double DevFee = 2.0;
        private int _apiPort;

        // command line parts
        private string _devices;

        public EwbfMiner(string uuid) : base(uuid)
        {}

        private string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.ZHash:
                    return "144_5";
                default:
                    return "";
            }
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();

            var algo = AlgorithmName(_algorithmType);

            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var split = urlWithPort.Split(':');
            var url = split[0];
            var port = split[1];

            var algorithmParam = $"--algo {algo}";
            if (_algorithmType == AlgorithmType.ZHash) algorithmParam += " --pers auto";

            var ret = $"{algorithmParam} --cuda_devices {_devices} --user {username} --server {url} --port {port} --pass x --api 127.0.0.1:{_apiPort} {_extraLaunchParameters}";
            if (!ret.Contains("--fee"))
            {
                ret += " --fee 0";
                DevFee = 0.0d;
            }

            return ret;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                JsonApiResponse resp = null;
                using (var client = new TcpClient("127.0.0.1", _apiPort))
                using (var nwStream = client.GetStream())
                {
                    var bytesToSend = Encoding.ASCII.GetBytes("{\"method\":\"getstat\"}\n");
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr);
                    client.Close();
                }

                // return if we got nothing
                var respOK = resp != null && resp.error == null;
                if (respOK == false) return api;

                var results = resp.result;

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;
                foreach (var gpu in gpus)
                {
                    var currentDevStats = results.Where(r => r.cudaid == gpu.ID).FirstOrDefault();
                    if (currentDevStats == null) continue;
                    totalSpeed += currentDevStats.speed_sps;
                    perDeviceSpeedInfo.Add(gpu.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentDevStats.speed_sps * (1 - DevFee * 0.01)) });
                    totalPowerUsage += (int)currentDevStats.gpu_power_usage;
                    perDevicePowerInfo.Add(gpu.UUID, (int)currentDevStats.gpu_power_usage);

                }
                api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
                api.PowerUsageTotal = totalPowerUsage;
                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 60, 90, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            // use demo user and disable the watchdog
            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC) + " --color 0 --boff";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            double benchHashesSum = 0;
            double benchHashResult = 0;
            int benchIters = 0;
            int targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 30d));

            const string totalSpeed = "Total speed:";
            //const string speedPerGPU = $"GPU{CUDA_D}"// per GPU if we would use multiple GPUs

            bp.CheckData = (string data) => {
                var containsSolRate = data.Contains(totalSpeed) && data.Contains("Sol");
                if (containsSolRate == false) return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) }, Success = false };
                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, totalSpeed);
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;
                if (!found) return new BenchmarkResult {AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) }, Success = false };

                // sum and return
                benchHashesSum += hashrate;
                benchIters++;

                benchHashResult = (benchHashesSum / benchIters) * (1 - DevFee * 0.01);

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = benchIters >= targetBenchIters
                };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "EWBF Equihash miner v0.6");
            var binPath = Path.Combine(pluginRootBins, "miner.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override void Init()
        {
            _devices = string.Join(" ", _miningPairs.Select(p => p.Device.ID));
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }
    }
}
