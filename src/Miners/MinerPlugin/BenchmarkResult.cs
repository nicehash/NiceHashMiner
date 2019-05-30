using System.Collections.Generic;

namespace MinerPlugin
{
    /// <summary>
    /// This class is used to report Benchmark results
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// AlgorithmTypeSpeeds is list of AlgorithmTypeSpeedPair (one or two elements depending on if algorithm is single or dual); for more info about this class <see cref="AlgorithmTypeSpeedPair"/>
        /// </summary>
        public IReadOnlyList<AlgorithmTypeSpeedPair> AlgorithmTypeSpeeds { get; set; } = null;
      
        /// <summary>
        /// Success tells us if benchmark finished (we can still have some unsuccessfull benchmarks with speeds - this indicates that the benchmarks weren't executed as planned)
        /// </summary>
        public bool Success  { get; set; } = false;
      
        /// <summary>
        /// ErrorMessage gives us error message in case of unsuccessfull benchmarking
        /// </summary>
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
