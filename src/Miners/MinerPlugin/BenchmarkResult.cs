using System.Collections.Generic;

namespace MinerPlugin
{
    /// <summary>
    /// This class is used to save Benchmark results
    /// It has 3 properties
    /// 1) AlgorithmTypeSpeeds which is list of AlgorithmTypeSpeedPair; for more info about this class <see cref="AlgorithmTypeSpeedPair"/>
    /// 2) Success which tells us if benchmark was successfull
    /// 3) ErrorMessage which gives us error message in case of unsuccessfull benchmarking
    /// </summary>
    public class BenchmarkResult
    {
        public IReadOnlyList<AlgorithmTypeSpeedPair> AlgorithmTypeSpeeds { get; set; } = null;
        public bool Success  { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// HasNonZeroSpeeds function returns true if any AlgorithmTypeSpeedPair in AlgorithmTypeSpeeds list contains speeds that are not zero
        /// </summary>
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
