using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Benchmarking
{
    // alias enum
    using BenchmarkSelection = AlgorithmBenchmarkSettingsType;
    public class BenchmarkStartSettings
    {
        public bool StartMiningAfterBenchmark { get; set; } = false;
        public BenchmarkPerformanceType BenchmarkPerformanceType { get; set; } = BenchmarkPerformanceType.Standard;
        public BenchmarkOption BenchmarkOption { get; set; } = BenchmarkOption.ZeroOnly;
    }
}
