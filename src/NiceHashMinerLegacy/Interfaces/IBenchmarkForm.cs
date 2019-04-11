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
        bool InBenchmark { get; }
        void SetCurrentStatus(ComputeDevice device, Algorithm algorithm, string status);
        void AddToStatusCheck(ComputeDevice device, Algorithm algorithm);
        void RemoveFromStatusCheck(ComputeDevice device, Algorithm algorithm);
        void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos);
        void StepUpBenchmarkStepProgress();
#endif
    }
}
