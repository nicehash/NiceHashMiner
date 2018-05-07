using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Benchmarking.BenchHelpers
{
    public class CpuBenchHelper
    {
        private readonly List<CpuBenchmark> _benchmarks = new List<CpuBenchmark>();

        private readonly int _maxThreads;
        public int Time;

        public CpuBenchHelper(int maxThreads)
        {
            _maxThreads = maxThreads;
        }

        public int LessTreads { get; private set; }

        public bool HasTest()
        {
            return LessTreads < _maxThreads;
        }

        public void SetNextSpeed(double speed)
        {
            if (HasTest())
            {
                _benchmarks.Add(new CpuBenchmark(LessTreads, speed));
                ++LessTreads;
            }
        }

        public void FindFastest()
        {
            _benchmarks.Sort((a, b) => -a.Benchmark.CompareTo(b.Benchmark));
        }

        public double GetBestSpeed()
        {
            return _benchmarks[0].Benchmark;
        }

        public int GetLessThreads()
        {
            return _benchmarks[0].LessTreads;
        }

        private class CpuBenchmark
        {
            public readonly double Benchmark;

            public readonly int LessTreads;

            public CpuBenchmark(int lt, double bench)
            {
                LessTreads = lt;
                Benchmark = bench;
            }
        }
    }
}
