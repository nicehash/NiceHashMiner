using NHM.Common.Enums;

namespace MinerPlugin
{
    /// <summary>
    /// This class is used to create a pair of AlgorithmType and its speed
    /// </summary>
    public class AlgorithmTypeSpeedPair
    {
        /// <summary>
        /// Constructor that takes <see cref="NHM.Common.Enums.AlgorithmType"/> and speed as arguments and create AlgorithmTypeSpeedPair with it
        /// </summary>
        public AlgorithmTypeSpeedPair(AlgorithmType algorithmType, double speed)
        {
            AlgorithmType = algorithmType;
            Speed = speed;
        }

        public AlgorithmType AlgorithmType { get; set; }
        public double Speed { get; set; }
    }
}
