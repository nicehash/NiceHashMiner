using MinerPlugin;
using MinerPluginToolkitV1;
using NHM.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRBMiner
{
    public class SRBMiner : MinerBase
    {
        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        protected override string MiningCreateCommandLine()
        {
            throw new NotImplementedException();
        }
    }
}
