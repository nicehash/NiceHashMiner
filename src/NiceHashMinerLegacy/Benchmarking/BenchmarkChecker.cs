using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Benchmarking
{
    public static class BenchmarkChecker
    {
        // IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks check if all enabled algorithms need benchmarking
        public static bool IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(ComputeDevice device)
        {
            var allEnabledAlgorithms = device.GetAlgorithmSettings().Where(algo => algo.Enabled);
            var allEnabledAlgorithmsWithoutBenchmarks = allEnabledAlgorithms.Where(algo => algo.BenchmarkNeeded);
            return allEnabledAlgorithms.Count() == allEnabledAlgorithmsWithoutBenchmarks.Count();
        }
    }
}
