using MinerPlugin;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    /// <summary>
    /// ExampleMiner class inherits IMiner to implement basic actions for miner
    /// </summary>
    public class ExampleMiner : IMiner
    {
        /// <summary>
        /// GetMinerStatsDataAsync function is used to retrieve data from miner API
        /// Through the course of function data is being filled to be returned at the end
        /// </summary>
        public async Task<ApiData> GetMinerStatsDataAsync()
        {
            var apiData = new ApiData();
            var speedsPerDevice = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
            var powerPerDevice = new Dictionary<string, int>();

            apiData.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(AlgorithmType.DaggerHashimoto, 107.5) };
            apiData.PowerUsageTotal = 228;

            speedsPerDevice.Add("gpuUUID1", new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(AlgorithmType.DaggerHashimoto, 65.1 ) });
            powerPerDevice.Add("gpuUUID1", 120);

            speedsPerDevice.Add("gpuUUID2", new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(AlgorithmType.DaggerHashimoto, 42.4 ) });
            powerPerDevice.Add("gpuUUID2", 108);

            apiData.AlgorithmSpeedsPerDevice = speedsPerDevice;
            apiData.PowerUsagePerDevice = powerPerDevice;

            return apiData;
        }

        /// <summary>
        /// InitMiningLocationAndUsername function is used to set mining location, username and password
        /// </summary>
        public void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x")
        {
            // Initialization of mining location, username and password is made here
        }

        /// <summary>
        /// In InitMiningPairs Device-Algorithm (miningPair) pairs are initialized
        /// </summary>
        public void InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
           // Initialization of mining pairs is made here
        }

        /// <summary>
        /// StartBenchmark starts benchmark process and awaits its results which are then used for accurate switching
        /// </summary>
        public Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // set the process and get the benchmark data
            // then return that data as BenchmarkResult
            var result = new Task<BenchmarkResult>(
                () => {
                    return new BenchmarkResult
                    {
                        AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(AlgorithmType.DaggerHashimoto, 62.5) },
                        Success = true,
                        ErrorMessage = ""
                    };
                });
            
            return result;
        }

        string miningState = "STOP";

        /// <summary>
        /// Start mining process when StartMining is called
        /// </summary>
        public void StartMining()
        {
            if (miningState == "START") return;
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = "miner.exe",
                    Arguments = "--algo dagger --url nicehashAlgoUrl --user userBTC",
                    WorkingDirectory = "WorkingDir"
                },
                EnableRaisingEvents = true
            };
            proc.Start();
            miningState = "START";
        }

        /// <summary>
        /// End mining process when StopMining is called
        /// </summary>
        public void StopMining()
        {
            if (miningState == "STOP") return;
            foreach (var process in Process.GetProcessesByName("miner"))
            {
                process.Kill();
            }
            miningState = "STOP";
        }
    }
}
