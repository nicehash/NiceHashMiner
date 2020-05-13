using NHM.Common.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NHM.MinerPlugin
{

    // TODO when we update to C#7 use tuple values or System.ValueTuple for .NET version that don't support C#7

    /// <summary>
    /// IMiner is the mandatory interface for all miners containing bare minimum functionalities
	/// It is used as miner process instance created by IMinerPlugin
    /// </summary>
    public interface IMiner
    {
        /// <summary>
        /// Sets mining pairs (<see cref="MiningPair"/>)
        /// </summary>
        void InitMiningPairs(IEnumerable<MiningPair> miningPairs);

        /// <summary>
        /// Sets Mining location and username; password is optional
        /// </summary>
        void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x");


        Task MinerProcessTask { get; }
        Task<object> StartMiningTask(CancellationToken stop);
        Task StopMiningTask();

        /// <summary>
        /// Returns Benchmark result (<see cref="BenchmarkResult"/>)
        /// </summary>
        Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard); // IBenchmarker

        /// <summary>
        /// Returns a task that retrives mining 
        /// </summary>
        Task<ApiData> GetMinerStatsDataAsync();
    }
}
