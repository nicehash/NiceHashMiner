using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPlugin.Toolkit;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;

namespace NBMiner
{
    public class NBMiner : MinerBase, IDisposable
    {
        private int _apiPort;
        private readonly string _uuid;
        private AlgorithmType _algorithmType;
        private readonly Dictionary<int, int> _cudaIDMap;
        private readonly HttpClient _http = new HttpClient();

        private string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.GrinCuckaroo29:
                        return "cuckaroo";
                    case AlgorithmType.GrinCuckatoo31:
                        return "cuckatoo";
                    case AlgorithmType.DaggerHashimoto:
                        return "ethash";
                    default:
                        return "";
                }
            }
        }

        public NBMiner(string uuid, Dictionary<int, int> cudaIDMap)
        {
            _uuid = uuid;
            _cudaIDMap = cudaIDMap;
        }

        public override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            throw new NotImplementedException();
        }

        protected override (string binPath, string binCwd) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "nbminer.exe");
            var binCwd = pluginRootBins;
            return (binPath, binCwd);
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            
            var devs = string.Join(",", _miningPairs.Select(p => _cudaIDMap[p.device.ID]));
            return $"-a {AlgoName} -o {url} -u {username} --api 127.0.0.1:{_apiPort} -d {devs} -RUN ";
        }
        
        public override async Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/api/v1/status");
                var summary = JsonConvert.DeserializeObject<NBMinerJsonResponse>(result);
                api.AlgorithmSpeedsTotal = new[] { (_algorithmType, summary.TotalHashrate ?? 0) };
            }
            catch (Exception e)
            {
            }

            return api;
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmType, ok) = _miningPairs.GetAlgorithmSingleType();
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
