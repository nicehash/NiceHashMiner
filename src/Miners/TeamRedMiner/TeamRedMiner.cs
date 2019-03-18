using MinerPlugin;
using MinerPlugin.Toolkit;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using static MinerPlugin.Toolkit.MinersApiPortsManager;
using System.IO;
using NiceHashMinerLegacy.Common;
using System.Net.Sockets;
using System.Text;

namespace TeamRedMiner
{
    public class TeamRedMiner : MinerBase
    {
        private int _apiPort;
        private readonly string _uuid;
        private readonly int _openClAmdPlatformNum;

        // can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;

        // command line parts
        private string _devicesOnPlatform;
        private string _extraLaunchParameters;

        public TeamRedMiner(string uuid, int openClAmdPlatformNum)
        {
            _uuid = uuid;
            _openClAmdPlatformNum = openClAmdPlatformNum;
        }

        private string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.CryptoNightV8:
                        return "cnv8";
                    case AlgorithmType.CryptoNightR:
                        return "cnr";
                    case AlgorithmType.Lyra2REv3:
                        return "lyra2rev3";
                    default:
                        return "";
                }
            }
        }

        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.CryptoNightV8:
                    case AlgorithmType.CryptoNightR:
                    case AlgorithmType.Lyra2REv3:
                        return 0.025d; // 2.5%
                    default:
                        return 0.03d; // 3.0%
                }
            }
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePortInRange(); // use the default range
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var cmd = $"-a {AlgoName} -o {url} -u {username} {_devicesOnPlatform}  --api_listen=127.0.0.1:{_apiPort} {_extraLaunchParameters}";
            return cmd;
        }

        // TODO implement this with JSON API
        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            return ad;
        }

        public async override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
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
                    benchmarkTime = 120;
                    break;
            }

            // use demo user and disable colorts so we can read from stdout
            var commandLine = CreateCommandLine(MinerToolkit.DemoUser) + " --disable_colors";
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            double benchHashesSum = 0;
            double benchHashResult = 0;
            int benchIters = 0;
            int targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 30d));

            string afterAlgoSpeed = $"{AlgoName}:";
            
            bp.CheckData = (string data) =>
            {
                var containsHashRate = data.Contains(afterAlgoSpeed) && data.Contains("GPU");
                if (containsHashRate == false) return (benchHashResult, false);
                var (hashrate, found) = MinerToolkit.TryGetHashrateAfter(data, afterAlgoSpeed);
                if (!found) return (benchHashResult, false);

                // sum and return
                benchHashesSum += hashrate;
                benchIters++;

                benchHashResult = (benchHashesSum / benchIters) * (1 - DevFee);

                var isFinished = benchIters >= targetBenchIters;
                return (benchHashResult, isFinished);
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override (string, string) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "teamredminer-v0.4.2-win");
            var binPath = Path.Combine(pluginRootBins, "teamredminer.exe");
            var binCwd = pluginRootBins;
            return (binPath, binCwd);
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmType, ok) = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");
            // all good continue on

            // init command line params parts
            var deviceIds = _miningPairs
                .Select(pair => pair.device.ID)
                .OrderBy(id => id)
                .Select(id => id.ToString());
            _devicesOnPlatform = $"--platform={_openClAmdPlatformNum} -d {string.Join(",", deviceIds)}";

            // TODO implement this later
            //_extraLaunchParameters;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }
    }
}
