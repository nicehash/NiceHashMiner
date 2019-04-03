using NiceHashMinerLegacy.Common.Enums;

namespace MinerPlugin
{
    public class AlgorithmTypeSpeedPair
    {
        public AlgorithmTypeSpeedPair(AlgorithmType algorithmType, double speed)
        {
            AlgorithmType = algorithmType;
            Speed = speed;
        }

        public AlgorithmType AlgorithmType { get; set; }
        public double Speed { get; set; }
    }
}
