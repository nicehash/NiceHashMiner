using System.Collections.Generic;

namespace MinerPlugin
{
    public class BenchmarkResult
    {
        public IReadOnlyList<AlgorithmTypeSpeedPair> AlgorithmTypeSpeeds { get; set; } = null;
        public bool Success  { get; set; } = false;
        public string ErrorMessage { get; set; } = "";
    }
}
