using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;

namespace MinerPlugin
{
    /// <summary>
    /// This class has 2 properties (Device and Algorithm) and is used to combine active devices and algorithms
    /// 1) Device property is mining device of type <see cref="BaseDevice"/>
    /// 2) Algorithm property is active algorithm of type <see cref="Algorithm."/>
    /// </summary>
    public class MiningPair
    {
        public BaseDevice Device { get; set; }
        public Algorithm Algorithm { get; set; }
    }
}
