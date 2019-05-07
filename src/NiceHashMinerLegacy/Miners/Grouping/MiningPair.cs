using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;

namespace NiceHashMiner.Miners.Grouping
{
    public class MiningPair
    {
        public ComputeDevice Device { get; private set; }
        public Algorithm Algorithm { get; private set; }

        public MiningPair(ComputeDevice d, Algorithm a)
        {
            Device = d;
            Algorithm = a;
        }
    }
}
