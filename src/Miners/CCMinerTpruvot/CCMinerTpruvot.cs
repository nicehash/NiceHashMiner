using MinerPlugin;
using MinerPluginToolkitV1.CCMinerCommon;
using NHM.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CCMinerTpruvot
{
    public class CCMinerTpruvot : CCMinerBase
    {
        public CCMinerTpruvot(string uuid) : base(uuid)
        { }

        protected override string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var ret = await base.StartBenchmark(stop, benchmarkType);
            if (_algorithmType == AlgorithmType.X16R)
            {
                try
                {
                    foreach (var infoPair in ret.AlgorithmTypeSpeeds)
                    {
                        infoPair.Speed = infoPair.Speed * 0.4563831001472754;
                    }
                }
                catch (Exception)
                {
                }
            }
            return ret;
        }
    }
}
