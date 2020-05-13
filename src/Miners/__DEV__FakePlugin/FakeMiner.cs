using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakePlugin
{
    /// <summary>
    /// ExampleMiner class inherits IMiner to implement basic actions for miner
    /// </summary>
    public class FakeMiner : MinerBase
    {
        #region members for simulation purposes
        string _devices;
        protected AlgorithmType _algorithmSecondType = AlgorithmType.NONE;

        Random _rand { get; } = new Random();

        //private enum MinerState
        //{
        //    STOPPED,
        //    MINING
        //}

        //MinerState _state = MinerState.STOPPED;

        #endregion members for simulation purposes

        public FakeMiner(string uuid):base(uuid){}

        protected virtual string AlgorithmName()
        {
            if (_algorithmSecondType != AlgorithmType.NONE)
            {
                var ret = $"{PluginSupportedAlgorithms.AlgorithmName(_algorithmType)}+{PluginSupportedAlgorithms.AlgorithmName(_algorithmSecondType)}";
                return ret;
            }
            // default single algo
            return PluginSupportedAlgorithms.AlgorithmName(_algorithmType);
        }

        public override async Task<ApiData> GetMinerStatsDataAsync()
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
                if (_algorithmSecondType != AlgorithmType.NONE)
                {
                    perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, speed), (_algorithmSecondType, speed) });
                }
                else
                {
                    perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, speed) });
                }
                perDevicePowerInfo.Add(deviceUUID, 108);
            }

            api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            api.PowerUsagePerDevice = perDevicePowerInfo;
            api.PowerUsageTotal = totalPowerUsage;

            return api;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            await Task.Delay(15000, stop);
            var success = !stop.IsCancellationRequested;
            double speed = success ? 1000 : 0; // everything is same speed

            // and return our result
            if (_algorithmSecondType != AlgorithmType.NONE)
            {
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<(AlgorithmType type, double speed)> { (_algorithmType, speed), (_algorithmSecondType, speed) },
                    Success = success,
                    ErrorMessage = ""
                };
            }
            else
            {
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<(AlgorithmType type, double speed)> { (_algorithmType, speed) },
                    Success = success,
                    ErrorMessage = ""
                };
            }
        }

        protected override void Init()
        {
            _devices = string.Join(",", _miningPairs.Select(p => $"{p.Device.Name}({p.Device.ID})"));
            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmSecondType = dualType.Item1;
            var ok = dualType.Item2;
            if (!ok) _algorithmSecondType = AlgorithmType.NONE;
        }

        protected override string MiningCreateCommandLine()
        {
            return $"-devices={_devices} -algorithm={AlgorithmName()} -miningLocaiton={_miningLocation} -username={_username} -UUID {_uuid}";
        }
    }
}
