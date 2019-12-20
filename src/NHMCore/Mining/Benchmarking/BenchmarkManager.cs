using NHMCore.Mining.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NHMCore.Mining.Benchmarking
{
    public static class BenchmarkManager
    {
        public static bool IsBenchmarking => BenchmarkingComputeDeviceHandler.IsBenchmarking;

#region Start/Stop methods

        private static bool ShouldBenchmark(AlgorithmContainer algo, BenchmarkOption benchmarkOption)
        {
            switch (benchmarkOption)
            {
                case BenchmarkOption.ZeroOnly:
                    return algo.BenchmarkNeeded;
                case BenchmarkOption.ReBecnhOnly:
                    return algo.IsReBenchmark;
                case BenchmarkOption.ZeroOrReBenchOnly:
                    return algo.BenchmarkNeeded || algo.IsReBenchmark;
            }
            return true;
        }

        // from deleted NHM.Extensions
        private static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            var queue = new Queue<T>();
            foreach (var el in source)
            {
                queue.Enqueue(el);
            }
            return queue;
        }

        // network benchmark starts benchmarking on a device
        // assume device is enabled and it exists
        public static void StartBenchmarForDevice(ComputeDevice device, BenchmarkStartSettings benchmarkStartSettings)
        {
            var startMiningAfterBenchmark = benchmarkStartSettings.StartMiningAfterBenchmark;
            var perfType = benchmarkStartSettings.BenchmarkPerformanceType;
            var benchmarkOption = benchmarkStartSettings.BenchmarkOption;
            var unbenchmarkedAlgorithms = device.AlgorithmSettings.Where(algo => algo.Enabled && ShouldBenchmark(algo, benchmarkOption)).ToQueue();
            BenchmarkingComputeDeviceHandler.BenchmarkDeviceAlgorithms(device, unbenchmarkedAlgorithms, perfType, startMiningAfterBenchmark);
        }

        public static Task StopBenchmarForDevice(ComputeDevice device)
        {
            return BenchmarkingComputeDeviceHandler.StopBenchmarkingDevice(device);
        }

        public static Task Stop()
        {
            return BenchmarkingComputeDeviceHandler.StopBenchmarkingAllDevices();
        }

#endregion

#region In-bench status updates

        public static void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos, bool startMiningAfterBenchmark = false)
        {            
            if (!IsBenchmarking)
            {
                // TODO now that we start mining afer benchmark maybe this line makes no sense? Check if we will always start mining after bench
                EthlargementIntegratedPlugin.Instance.Stop();
            }

            if (startMiningAfterBenchmark)
            {
                // TODO attempt to schedule devices to start in batch
                ApplicationStateManager.StartDeviceTask(device, true);
            }
        }

        public static void SetCurrentStatus(ComputeDevice dev, AlgorithmContainer algo, string status)
        {
            //var args = new AlgoStatusEventArgs(dev, algo, status);
            // TODO append to NotificationInfo instance 
            //OnAlgoStatusUpdate?.Invoke(null, args);
        }

#endregion
    }
}
