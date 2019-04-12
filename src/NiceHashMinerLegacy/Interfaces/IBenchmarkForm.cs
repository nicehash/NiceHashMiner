using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;

namespace NiceHashMiner.Interfaces
{
    public interface IBenchmarkForm
    {
#if TESTNET || TESTNETDEV
        void EndBenchmark(bool hasFailedAlgos);
        bool StartMiningOnFinish { get; }
#else
        bool InBenchmark { get; } // BenchmarkManager
        void SetCurrentStatus(ComputeDevice device, Algorithm algorithm, string status); // BenchmarkManager
        void AddToStatusCheck(ComputeDevice device, Algorithm algorithm); // BenchmarkManager
        void RemoveFromStatusCheck(ComputeDevice device, Algorithm algorithm); // BenchmarkManager
        void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos); // BenchmarkManager
        void StepUpBenchmarkStepProgress(); // BenchmarkManager
#endif
    }
}
