using System.Collections.Generic;

namespace MinerPlugin
{
    public class BenchmarkResult
    {
        public IReadOnlyList<AlgorithmTypeSpeedPair> AlgorithmTypeSpeeds { get; set; } = null;
        public bool Success  { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        public bool HasNonZeroSpeeds()
        {
            if (AlgorithmTypeSpeeds == null) return false;
            foreach (var speedPair in AlgorithmTypeSpeeds)
            {
                if (speedPair.Speed > 0) return true;
            }
            return false;
        }
    }
}
