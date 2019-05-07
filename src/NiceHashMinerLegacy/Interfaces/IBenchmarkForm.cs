using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;

namespace NiceHashMiner.Interfaces
{
    public interface IBenchmarkForm
    {
        //void EndBenchmark(bool hasFailedAlgos);
        bool StartMiningOnFinish { get; }
    }
}
