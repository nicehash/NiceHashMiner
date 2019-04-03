using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace TTMiner
{
    public class TTMiner : MinerBase, IDisposable, IAfterStartMining
    {
        private int _apiPort;
        private readonly string _uuid;
        private AlgorithmType _algorithmType;
        //private readonly Dictionary<int, int> _cudaIDMap;
        //private readonly HttpClient _http = new HttpClient();

        private string _devices;
        private string _extraLaunchParameters = "";

        // TODO figure out how to fix API workaround without this started time
        private DateTime _started;

        private class JsonApiResponse
        {
#pragma warning disable IDE1006 // Naming Styles
            public List<string> result { get; set; }
            public int id { get; set; }
            public object error { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        private string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.MTP:
                        return "mtp";
                    case AlgorithmType.Lyra2REv3:
                        return "LYRA2V3";
                    default:
                        return "";
                }
            }
        }

        private double DevFee
        {
            get
            {
                return 0.01d; // 1% for all
            }
        }

        public TTMiner(string uuid)
        {
            _uuid = uuid;
        }

        public override async Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            int benchTime;
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchTime = 20;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchTime = 120;
                    break;
                default:
                    benchTime = 60;
                    break;
            }

            var commandLine = CreateCommandLine(MinerToolkit.DemoUser);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;
            var targetBenchIters = 2; //Math.Max(1, (int)Math.Floor(benchTime / 20d));

            bp.CheckData = (data) =>
            {
                var (hashrate, found) = data.ToLower().TryGetHashrateAfter("]:");
                if (data.Contains("LastShare") && data.Contains("GPU[") && found && hashrate > 0)
                {
                    benchHashes += hashrate;
                    benchIters++;
                    benchHashResult = (benchHashes / benchIters) * (1.0d - DevFee);
                }
                return (benchHashResult, benchIters >= targetBenchIters);
            };

            var timeout = TimeSpan.FromSeconds(benchTime + 5);
            var benchWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, timeout, benchWait, stop);
            return await t;
        }

        protected override (string binPath, string binCwd) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "TT-Miner.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"-a {AlgoName} -url {url} -u {username} -d {_devices} --api-bind 127.0.0.1:{_apiPort} {_extraLaunchParameters}";
            return cmd;
        }

        public override async Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            var elapsedSeconds = DateTime.Now.Subtract(_started).Seconds;
            if (elapsedSeconds < 15)
            {
                return api;
            }
            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes("{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}\n");
                using (var client = new TcpClient("127.0.0.1", _apiPort))
                using (var nwStream = client.GetStream())
                {
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    //Helpers.ConsolePrint(MinerTag(), "respStr: " + respStr);
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr);
                    // TODO
                    //api.AlgorithmSpeedsTotal = new[] { (_algorithmType, resp.TotalHashrate ?? 0) };
                    if (resp != null && resp.error == null)
                    {
                        //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp != null && resp.error == null");
                        if (resp.result != null && resp.result.Count > 4)
                        {
                            var speeds = resp.result[3].Split(';');
                            var totalSpeed = 0d;
                            foreach (var speed in speeds)
                            {
                                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "foreach (var speed in speeds) {");
                                double tmpSpeed;
                                try
                                {
                                    tmpSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                                }
                                catch
                                {
                                    tmpSpeed = 0;
                                }

                                totalSpeed += tmpSpeed;
                            }
                            var total = new List<(AlgorithmType, double)>();
                            total.Add((_algorithmType, totalSpeed));
                            api.AlgorithmSpeedsTotal = total; //new List<(AlgorithmType, double)>((_algorithmType, totalSpeed));
                        }

                        //if (ad.Speed == 0)
                        //{
                        //    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                        //}

                        ////// some clayomre miners have this issue reporting negative speeds in that case restart miner
                        ////if (ad.Speed < 0)
                        ////{
                        ////    Helpers.ConsolePrint(MinerTag(), "Reporting negative speeds will restart...");
                        ////    Restart();
                        ////}
                    }
                }
            }
            catch (Exception ex)
            {
                //Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + ex.Message);
            }

            return api;
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmType, ok) = _miningPairs.GetAlgorithmSingleType();
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");

            // Order pairs and parse ELP
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.device.ID.CompareTo(b.device.ID));
            _devices = string.Join(" ", orderedMiningPairs.Select(p => p.device.ID));
            if (MinerOptionsPackage != null)
            {
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public void AfterStartMining()
        {
            _started = DateTime.Now;
        }

        public void Dispose()
        {
        }
    }
}
