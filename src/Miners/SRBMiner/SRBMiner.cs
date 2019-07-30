using MinerPlugin;
using MinerPluginToolkitV1;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRBMiner
{
    public class SRBMiner : MinerBase
    {
        private int _apiPort;
        private double DevFee = 0.85;
        private string _devices;

        public SRBMiner(string uuid) : base(uuid)
        {
        }

        private string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.CryptoNightR:
                        return "cryptonight_r";
                    default:
                        return "";
                }
            }
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "SRBMiner-CN-V1-9-3");
            var binPath = Path.Combine(pluginRootBins, "SRBMiner-CN.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        public override Task<ApiData> GetMinerStatsDataAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            throw new NotImplementedException();
        }

        protected override void Init()
        {
            var miningPairsList = _miningPairs.ToList();
            _devices = string.Join(",", miningPairsList.Select(p => p.Device.ID));
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];
            var cmd = $"--ccryptonighttype {AlgoName} --cpool {url}:{port} --cwallet {username} --cgpuid {_devices} --cnicehash true --disablegpuwatchdog --apienable --apiport {_apiPort} {_extraLaunchParameters}";
            return cmd;
        }
    }
}
