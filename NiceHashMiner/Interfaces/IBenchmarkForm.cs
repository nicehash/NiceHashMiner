using System.Collections.Generic;
using NiceHashMiner.Algorithms;

namespace NiceHashMiner.Interfaces
{
    public interface IBenchmarkForm
    {
        void EndBenchmark(bool hasFailedAlgos);

        bool StartMiningOnFinish { get; }
    }
}
