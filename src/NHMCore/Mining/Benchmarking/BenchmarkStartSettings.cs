using NHM.Common.Enums;

namespace NHMCore.Mining.Benchmarking
{
    // alias enum
    public class BenchmarkStartSettings
    {
        public bool StartMiningAfterBenchmark { get; set; } = false;
        public BenchmarkPerformanceType BenchmarkPerformanceType { get; set; } = BenchmarkPerformanceType.Standard;
        public BenchmarkOption BenchmarkOption { get; set; } = BenchmarkOption.ZeroOnly;
    }
}
