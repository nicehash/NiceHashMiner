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

        /// <summary>
        /// Sets mining pairs (<see cref="MiningPair"/>)
        /// </summary>
        void InitMiningPairs(IEnumerable<MiningPair> miningPairs);


        // change miningLocation with URL, optional password
        /// <summary>
        /// Sets Mining location and username; password is optional
        /// </summary>
        void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x");

        // TODO add optional before and after start cases
        void StartMining();
        void StopMining();

        // TODO for mining and benchmarking sys vars should be set

        // TODO no dual algorithm support
        // TODO start benchmark
        // TODO when we update to C#7 use tuple values

        /// <summary>
        /// Returns Benchmark result (<see cref="BenchmarkResult"/>)
        /// </summary>
        Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard); // IBenchmarker
        // TODO add interface for parallel benchmarking
        //Task StopBenchmark();

        /// <summary>
        /// Returns a task that retrives mining 
        /// </summary>
        Task<ApiData> GetMinerStatsDataAsync();
    }
}
