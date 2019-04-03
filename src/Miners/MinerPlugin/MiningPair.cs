using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;

namespace MinerPlugin
{
    public class MiningPair
    {
        public BaseDevice Device { get; set; }
        public Algorithm Algorithm { get; set; }
    }
}
