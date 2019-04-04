using MinerPlugin;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.ClaymoreCommon
{

    public class ClaymoreBase : MinerBase
    {
        private int _apiPort;
        protected readonly string _uuid;

        // this is second algorithm - if this is null only dagger is being mined
        private AlgorithmType _algorithmSingleType;
        private AlgorithmType _algorithmDualType;

        private string _devices;
        private string _extraLaunchParameters = "";

        // command line parts
        private string _platform;

        public ClaymoreBase(string uuid)
        {
            _uuid = uuid;
        }

        private static int GetPlatformIDForType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.AMD:
                    return 1;
                case DeviceType.NVIDIA:
                    return 2;
                default:
                    return 3;
            }
        }

        protected virtual string SingleAlgoName
        {
            get
            {
                switch (_algorithmSingleType)
                {
                    case AlgorithmType.DaggerHashimoto:
                        return "eth";
                    default:
                        return "";
                }
            }
        }

        protected virtual string DualAlgoName
        {
            get
            {
                switch (_algorithmDualType)
                {
                    case AlgorithmType.DaggerDecred:
                        return "dcr";
                    case AlgorithmType.DaggerBlake2s:
                        return "b2s";
                    case AlgorithmType.DaggerKeccak:
                        return "kc";
                    default:
                        return "";
                }
            }
        }

        private double DevFee
        {
            get
            {
                return 1.0;
            }
        }

        private double DualDevFee
        {
            get
            {
                return 0.0;
            }
        }

        public bool IsDual()
        {
            return (_algorithmDualType != AlgorithmType.NONE);
        }

        private class JsonApiResponse
        {
#pragma warning disable IDE1006 // Naming Styles
            public List<string> result { get; set; }
            public int id { get; set; }
            public object error { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();

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
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr);
                }
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", respStr);
                if (resp != null && resp.error == null)
                {
                    //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp != null && resp.error == null");
                    if (resp.result != null && resp.result.Count > 4)
                    {
                        //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp.result != null && resp.result.Count > 4");
                        var speeds = resp.result[3].Split(';');
                        var secondarySpeeds = (IsDual()) ? resp.result[5].Split(';') : new string[0];
                        var primarySpeed = 0d;
                        var secondarySpeed = 0d;
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

                            primarySpeed += tmpSpeed;
                        }

                        foreach (var speed in secondarySpeeds)
                        {
                            double tmpSpeed;
                            try
                            {
                                tmpSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                tmpSpeed = 0;
                            }

                            secondarySpeed += tmpSpeed;
                        }
                        var totalPrimary = new List<AlgorithmTypeSpeedPair>();
                        var totalSecondary = new List<AlgorithmTypeSpeedPair>();
                        totalPrimary.Add(new AlgorithmTypeSpeedPair(AlgorithmType.DaggerHashimoto, primarySpeed));
                        totalSecondary.Add(new AlgorithmTypeSpeedPair(_algorithmDualType, secondarySpeed));

                        ad.AlgorithmSpeedsTotal = totalPrimary;
                        ad.AlgorithmSecondarySpeedsTotal = totalSecondary;
                        //ad.Speed *= ApiReadMult;
                        //ad.SecondarySpeed *= ApiReadMult;
                    }
                }
            }
            catch (Exception ex)
            {
                //Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + ex.Message);
            }
            return ad;
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmSingleType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");

            //doesn't work - will always return NONE
            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmDualType = dualType.Item1;
            ok = dualType.Item2;
            if (!ok) _algorithmDualType = AlgorithmType.NONE;
            // all good continue on

            // Order pairs and parse ELP
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            _devices = string.Join("", orderedMiningPairs.Select(p => p.Device.ID));
            _platform = $"{GetPlatformIDForType(orderedMiningPairs.First().Device.DeviceType)}";

            if (MinerOptionsPackage != null)
            {
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var benchmarkTime = 90; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 90;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 180;
                    break;
            }

            var commandLine = CreateCommandLine(MinerToolkit.DemoUser);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            var benchHashesFirst = 0d;
            var benchIters = 0;
            var benchHashResultFirst = 0d;
            var benchHashesSecond = 0d;
            var benchHashResultSecond = 0d;
            //var afterSingle = $"{SingleAlgoName.ToUpper()} - Total Speed:";
            var afterSingle = $"GPU{_devices}";
            var afterDual = $"{DualAlgoName.ToUpper()} - Total Speed:";
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                // if (_algorithmDualType == AlgorithmType.NONE)
                // {
                Console.WriteLine("Data za benchmark: ", data);
                    var hashrateFoundPairFirst = data.TryGetHashrateAfter(afterSingle);
                    var hashrateFirst = hashrateFoundPairFirst.Item1;
                    var foundFirst = hashrateFoundPairFirst.Item2;
                    benchHashesFirst += hashrateFirst;
                    benchIters++;

                    benchHashResultFirst = (benchHashesFirst / benchIters) * (1 - DevFee * 0.01);
                    return new BenchmarkResult
                    {
                        AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmSingleType, benchHashResultFirst) },
                        Success = benchIters >= targetBenchIters
                    };
                /*} else
                {
                    var hashrateFoundPairFirst = data.TryGetHashrateAfter(afterSingle);
                    var hashrateFirst = hashrateFoundPairFirst.Item1;
                    var foundFirst = hashrateFoundPairFirst.Item2;
                    benchHashesFirst += hashrateFirst;
                    benchIters++;

                    benchHashResultFirst = (benchHashesFirst / benchIters) * (1 - DevFee * 0.01);

                    var hashrateFoundPairSecond = data.TryGetHashrateAfter(afterDual);
                    var hashrateSecond = hashrateFoundPairSecond.Item1;
                    var foundSecond = hashrateFoundPairSecond.Item2;
                    benchHashesSecond += hashrateSecond;
                    benchIters++;

                    benchHashResultSecond = (benchHashesSecond / benchIters) * (1 - DevFee * 0.01);
                    return new BenchmarkResult
                    {
                        AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmSingleType, benchHashResultFirst), new AlgorithmTypeSpeedPair(_algorithmDualType, benchHashResultSecond) },
                        Success = benchIters >= targetBenchIters
                    };
                }*/
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "EthDcrMiner64.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        private string CreateCommandLine(string username)
        {
            var urlFirst = StratumServiceHelpers.GetLocationUrl(_algorithmSingleType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = "";
            if (_algorithmDualType == AlgorithmType.NONE) //noDual
            {
                //cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dbg -1 {_extraLaunchParameters} -wd 0";
                cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dbg 1 {_extraLaunchParameters} -wd 0";
            }
            else
            {
                var urlSecond = StratumServiceHelpers.GetLocationUrl(_algorithmDualType, _miningLocation, NhmConectionType.STRATUM_TCP);
                cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dcoin {DualAlgoName} -dpool {urlSecond} -dwal {username} -dpsw x -dbg 1 {_extraLaunchParameters} -wd 0";
                //cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dcoin {DualAlgoName} -dpool {urlSecond} -dwal {username} -dpsw x -dbg -1 {_extraLaunchParameters} -wd 0";
            }

            return cmd;
        }

        protected override string MiningCreateCommandLine()
        {
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange();
            return CreateCommandLine(_username) + $" -mport 127.0.0.1:-{_apiPort}";
        }
    }
}
