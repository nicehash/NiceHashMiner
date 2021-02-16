using NHMCore.Mining.Plugins;
using System.Linq;
using System.Threading.Tasks;

namespace NHMCore.Mining.Benchmarking
{
    internal static class BenchmarkManager
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

        // network benchmark starts benchmarking on a device
        // assume device is enabled and it exists
        internal static void StartBenchmarForDevice(ComputeDevice device, BenchmarkStartSettings benchmarkStartSettings)
        {
            var startMiningAfterBenchmark = benchmarkStartSettings.StartMiningAfterBenchmark;
            var perfType = benchmarkStartSettings.BenchmarkPerformanceType;
            var benchmarkOption = benchmarkStartSettings.BenchmarkOption;
            var unbenchmarkedAlgorithms = device.AlgorithmSettings.Where(algo => algo.Enabled && ShouldBenchmark(algo, benchmarkOption)).ToArray();
            BenchmarkingComputeDeviceHandler.BenchmarkDeviceAlgorithms(device, unbenchmarkedAlgorithms, perfType, startMiningAfterBenchmark);
        }

        internal static Task StopBenchmarForDevice(ComputeDevice device)
        {
            return BenchmarkingComputeDeviceHandler.StopBenchmarkingDevice(device);
        }

        internal static Task Stop()
        {
            return BenchmarkingComputeDeviceHandler.StopBenchmarkingAllDevices();
        }

        #endregion

        #region In-bench status updates

        internal static void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos, bool startMiningAfterBenchmark = false)
        {
            if (!IsBenchmarking)
            {
                // TODO now that we start mining afer benchmark maybe this line makes no sense? Check if we will always start mining after bench
                EthlargementIntegratedPlugin.Instance.Stop();
            }

            if (failedAlgos)
            {
                Notifications.AvailableNotifications.CreateFailedBenchmarksInfo(device);
            }

            if (startMiningAfterBenchmark)
            {
                // TODO attempt to schedule devices to start in batch
                _ = ApplicationStateManager.StartDeviceTask(device, true);
            }
        }
        #endregion
    }
}
