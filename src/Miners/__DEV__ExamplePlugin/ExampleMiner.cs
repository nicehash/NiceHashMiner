using NHM.Common.Enums;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    /// <summary>
    /// ExampleMiner class inherits IMiner to implement basic actions for miner
    /// </summary>
    public class ExampleMiner : IMiner
    {
        #region members for simulation purposes
        List<MiningPair> _miningPairs;
        string _miningLocation = "location";
        string _username = "DEMO";
        AlgorithmType _algorithmType;

        Process _miningProcess;
        Random _rand { get; } = new Random();

        Task IMiner.MinerProcessTask => throw new NotImplementedException();

        private enum MinerState
        {
            STOPPED,
            MINING
        }

        MinerState _state = MinerState.STOPPED;

        #endregion members for simulation purposes

        /// <summary>
        /// GetMinerStatsDataAsync function is used to retrieve data from miner API
        /// Through the course of function data is being filled to be returned at the end
        /// </summary>
        public async Task<ApiData> GetMinerStatsDataAsync()
        {
            // simulate API delay
            await Task.Delay(150);

            var api = new ApiData();
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();
            var totalSpeed = 0d;
            var totalPowerUsage = 0;

            foreach (var mp in _miningPairs)
            {
                var speedVariation = _rand.Next(-100, 100);
                var speed = 1000 + speedVariation;
                var powerVariation = _rand.Next(-10, 10);
                var power = 100 + powerVariation;
                totalSpeed += speed;
                totalPowerUsage += power;

                var deviceUUID = mp.Device.UUID;
                perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, speed) });
                perDevicePowerInfo.Add(deviceUUID, 108);
            }

            api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            api.PowerUsagePerDevice = perDevicePowerInfo;
            api.PowerUsageTotal = totalPowerUsage;

            return api;
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
            _miningPairs = miningPairs.ToList();
            _algorithmType = _miningPairs.First().Algorithm.IDs.First();
        }

        /// <summary>
        /// StartBenchmark starts benchmark process and awaits its results which are then used for accurate switching
        /// </summary>
        public async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // set the process and get the benchmark data
            // then return that data as BenchmarkResult


            // here we simulate benchmark work
            await Task.Delay(15000, stop);
            var success = !stop.IsCancellationRequested;
            double speed = success ? 1000 : 0; // everything is same speed

            // and return our result
            return new BenchmarkResult
            {
                AlgorithmTypeSpeeds = new List<(AlgorithmType type, double speed)> { (_algorithmType, speed) },
                Success = success,
                ErrorMessage = ""
            };
        }

        /// <summary>
        /// Start mining process when StartMining is called
        /// </summary>
        public void StartMining()
        {
            if (_state == MinerState.MINING) return;
            // prepare a bat script as this will simulate our miner
            var batFileContents = new StringBuilder();
            batFileContents.AppendLine($"echo \"{"I am a fake ExampleMiner from ExamplePlugin"}\"");
            batFileContents.AppendLine($"echo \"{$"Currently mining algorithm {_algorithmType.ToString()}"}\"");
            batFileContents.AppendLine($"echo \"{$"On mining location {_miningLocation}"}\"");
            batFileContents.AppendLine($"echo \"{$"With username {_username}"}\"");
            batFileContents.AppendLine($"echo \"{$"With devices:"}\"");
            foreach (var mp in _miningPairs)
            {
                batFileContents.AppendLine($"echo \"    {$"{mp.Device.Name}:{mp.Device.ID}:{mp.Device.UUID}"}\"");
            }
            batFileContents.AppendLine($"echo \"{"Mining..."}\"");
            batFileContents.AppendLine("pause");

            var tempMiner = Path.GetTempFileName().Replace(".tmp", ".bat");
            File.WriteAllText(tempMiner, batFileContents.ToString());
            _miningProcess = Process.Start(new ProcessStartInfo("cmd.exe", "/c " + tempMiner));
            _state = MinerState.MINING;
        }

        /// <summary>
        /// End mining process when StopMining is called
        /// </summary>
        public void StopMining()
        {
            if (_state == MinerState.STOPPED) return;
            try
            {
                _miningProcess.Kill();
                _miningProcess.Dispose();
            }
            catch (Exception)
            {
            }
            _state = MinerState.STOPPED;
        }

        void IMiner.InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            throw new NotImplementedException();
        }

        void IMiner.InitMiningLocationAndUsername(string miningLocation, string username, string password)
        {
            throw new NotImplementedException();
        }

        Task<object> IMiner.StartMiningTask(CancellationToken stop)
        {
            throw new NotImplementedException();
        }

        Task IMiner.StopMiningTask()
        {
            throw new NotImplementedException();
        }

        Task<BenchmarkResult> IMiner.StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType)
        {
            throw new NotImplementedException();
        }

        Task<ApiData> IMiner.GetMinerStatsDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
