using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPlugin
{

    // IMiner is the mandatory and common interface for all miners.

    /// <summary>
    /// IMiner is the mandatory interface for all miners
    /// </summary>
    public interface IMiner
    {
        //// might not need it
        //bool IsRunning { get; } // this one whould be temp or in a different version
        //// might not need it
        //// enumerable must be read only 
        //IEnumerable<AlgorithmType> CurrentAlgorithms { get; }

        void InitMiningPairs(IEnumerable<(BaseDevice device, Algorithm algorithm)> miningPairs);


        // change miningLocation with URL, optional password
        void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x");

        // TODO add optional before and after start cases
        void StartMining();
        void StopMining();

        // TODO for mining and benchmarking sys vars should be set

        // TODO no dual algorithm support
        // TODO start benchmark
        // TODO when we update to C#7 use tuple values
        Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard); // IBenchmarker
        // TODO add interface for parallel benchmarking
        //Task StopBenchmark();

        /// <summary>
        /// Returns a task that retrives mining 
        /// </summary>
        /// <returns></returns>
        Task<ApiData> GetMinerStatsDataAsync();
    }
}
