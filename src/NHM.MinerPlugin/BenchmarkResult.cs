using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHM.MinerPlugin
{
    /// <summary>
    /// This class is used to report Benchmark results
    /// </summary>
    public class BenchmarkResult
    {
        public IReadOnlyList<ApiData> BenchmarkApiData { get; set; } = null;
        public IReadOnlyList<string> BenchmarkReadCommandLines { get; set; } = null;

        /// <summary>
        /// AlgorithmTypeSpeeds is list of AlgorithmTypeSpeedPair (one or two elements depending on if algorithm is single or dual); for more info about this class <see cref="AlgorithmTypeSpeedPair"/>
        /// </summary>
        public IReadOnlyList<(AlgorithmType type, double speed)> AlgorithmTypeSpeeds { get; set; } = null;

        /// <summary>
        /// Success tells us if benchmark finished (we can still have some unsuccessfull benchmarks with speeds - this indicates that the benchmarks weren't executed as planned)
        /// </summary>
        public bool Success { get; set; } = false;

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
                if (speedPair.speed > 0) return true;
            }
            return false;
        }
    }
}
