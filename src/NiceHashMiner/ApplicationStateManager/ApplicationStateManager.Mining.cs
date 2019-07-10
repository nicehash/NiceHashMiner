using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Common.Enums;
using NiceHashMiner.Benchmarking;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using NHM.Common;
using NiceHashMiner.Configs;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        public static string GetUsername()
        {
            if (MiningState.Instance.IsDemoMining) {
                return DemoUser.BTC;
            }

            var (btc, worker, _unused) = ConfigManager.GeneralConfig.GetCredentials();

            // TESTNET
#if (TESTNET || TESTNETDEV || PRODUCTION_NEW)
#if SEND_STRATUM_WORKERNAME
            if (worker.Length > 0 && CredentialValidators.ValidateWorkerName(worker))
            {
                return $"{btc}.{worker}${RigID}";
            }
#endif

            return $"{btc}${RigID}";
#else
            // PRODUCTION
            if (worker.Length > 0 && CredentialValidators.ValidateWorkerName(worker))
            {
                return $"{btc}.{worker}";
            }

            return $"{btc}";
#endif
        }

        private static void UpdateDevicesToMine()
        {
            var allDevs = AvailableDevices.Devices;
            var devicesToMine = allDevs.Where(dev => dev.State == DeviceState.Mining).ToList();
            if (devicesToMine.Count > 0) {
                StartMining();
                MiningManager.UpdateMiningSession(devicesToMine, GetUsername());
            } else {
                StopMining(false);
            }
        }

        private static void RestartMinersIfMining()
        {
            // if mining update the mining manager
            if (MiningState.Instance.IsCurrentlyMining)
            {
                MiningManager.RestartMiners(GetUsername());
            }
        }

        public static void ResumeMiners()
        {
            if (_resumeOldState)
            {
                _resumeOldState = false;
                foreach (var dev in _resumeDevs)
                {
                    StartDevice(dev);
                }
                _resumeDevs.Clear();
            }
            else
            {
                RestartMinersIfMining();
            }
        }

        private static bool _resumeOldState = false;
        private static HashSet<ComputeDevice> _resumeDevs = new HashSet<ComputeDevice>();
        public static void PauseMiners()
        {
            _resumeOldState = CurrentForm == CurrentFormState.Main;
            foreach(var dev in AvailableDevices.Devices)
            {
                if (dev.State == DeviceState.Benchmarking || dev.State == DeviceState.Mining)
                {
                    _resumeDevs.Add(dev);
                    StopAllDevice();
                }
            }
        }

        // TODO add check for any enabled algorithms
        public static (bool started, string failReason) StartAllAvailableDevices(bool isRpcCall = false)
        {
            var allDevs = AvailableDevices.Devices;
            var devicesToStart = allDevs.Where(dev => dev.State == DeviceState.Stopped);
            if (devicesToStart.Count() == 0) {
                return (false, "there are no new devices to start");
            }
            // TODO for now no partial success so if one fails send back that everything fails
            var started = true;
            var failReason = "";

            // TODO we have a BUG HERE if device enabled with all disabled algorithms
            var devicesToBenchmark = devicesToStart.Where(dev => dev.AllEnabledAlgorithmsWithoutBenchmarks() && dev.AnyAlgorithmEnabled());
            var devicesToReBenchmark = devicesToStart.Where(dev => dev.HasEnabledAlgorithmsWithReBenchmark() && !devicesToBenchmark.Contains(dev));
            foreach (var dev in devicesToBenchmark) {
                dev.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(dev, new BenchmarkStartSettings
                {
                    StartMiningAfterBenchmark = true,
                    BenchmarkPerformanceType = BenchmarkPerformanceType.Standard,
                    BenchmarkOption = BenchmarkOption.ZeroOrReBenchOnly
                });
            }
            foreach (var dev in devicesToReBenchmark)
            {
                dev.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(dev, new BenchmarkStartSettings
                {
                    StartMiningAfterBenchmark = true,
                    BenchmarkPerformanceType = BenchmarkPerformanceType.Standard,
                    BenchmarkOption = BenchmarkOption.ReBecnhOnly
                });
            }

            var devicesInErrorState = devicesToStart.Where(dev => !dev.AnyAlgorithmEnabled() || dev.AllEnabledAlgorithmsZeroPaying()).ToList();
            var devicesInErrorStateUUIDs = devicesInErrorState.Select(dev => dev.Uuid);
            foreach (var dev in devicesInErrorState)
            {
                dev.State = DeviceState.Error;
            }

            // TODO check count
            var devicesToMine = devicesToStart.Where(dev => !dev.AllEnabledAlgorithmsWithoutBenchmarks() && !devicesInErrorStateUUIDs.Contains(dev.Uuid)).ToList();
            foreach (var dev in devicesToMine) {
                dev.State = DeviceState.Mining;
            }
            UpdateDevicesToMine();
            if (!isRpcCall) {
                NiceHashStats.StateChanged();
            }
            RefreshDeviceListView?.Invoke(null, null);

            return (started, "");
        }

        public static (bool started, string failReason) StartDevice(ComputeDevice device, bool skipBenhcmakrk = false)
        {
            // we can only start a device it is already stopped
            if (device.State != DeviceState.Stopped && !skipBenhcmakrk)
            {
                return (false, "Device already started");
            }

            var started = true;
            var failReason = "";
            var isErrorState = !device.AnyAlgorithmEnabled();
            var isAllZeroPayingState = device.AllEnabledAlgorithmsZeroPaying();
            // check if device has any benchmakrs
            var needsBenchmark = device.AllEnabledAlgorithmsWithoutBenchmarks();
            var needsReBenchmark = device.HasEnabledAlgorithmsWithReBenchmark();
            if (isErrorState || isAllZeroPayingState)
            {
                device.State = DeviceState.Error;
                started = false;
                failReason = isAllZeroPayingState ? "No enabled algorithm is profitable" : "Cannot start a device with all disabled algoirhtms";
            }
            else if (needsBenchmark && !skipBenhcmakrk)
            {
                device.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(device, new BenchmarkStartSettings {
                    StartMiningAfterBenchmark = true,
                    BenchmarkPerformanceType = BenchmarkPerformanceType.Standard,
                    BenchmarkOption = BenchmarkOption.ZeroOnly
                });
            }
            else if (needsReBenchmark && !skipBenhcmakrk)
            {
                device.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(device, new BenchmarkStartSettings
                {
                    StartMiningAfterBenchmark = true,
                    BenchmarkPerformanceType = BenchmarkPerformanceType.Standard,
                    BenchmarkOption = BenchmarkOption.ReBecnhOnly
                });
            }
            else
            {
                device.State = DeviceState.Mining;
                UpdateDevicesToMine();
            }

            RefreshDeviceListView?.Invoke(null, null);
            NiceHashStats.StateChanged();

            return (started, failReason);
        }

        public static (bool stopped, string failReason) StopAllDevice() {
            var allDevs = AvailableDevices.Devices;
            // TODO when starting and stopping we are not taking Pending and Error states into account
            var devicesToStop = allDevs.Where(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking);
            if (devicesToStop.Count() == 0) {
                return (false, "No new devices to stop");
            }
            var devicesToStopBenchmarking = devicesToStop.Where(dev => dev.State == DeviceState.Benchmarking);
            if (devicesToStopBenchmarking.Count() > 0) {
                BenchmarkManager.StopBenchmarForDevices(devicesToStopBenchmarking);
            }
            var devicesToStopMining = devicesToStop.Where(dev => dev.State == DeviceState.Mining);
            if (devicesToStopMining.Count() > 0) {
                foreach (var stopDevice in devicesToStopMining) {
                    stopDevice.State = DeviceState.Stopped;
                }
                UpdateDevicesToMine();
            }

            // TODO for now no partial success so if one fails send back that everything fails
            var stopped = true;
            var failReason = "";
            //// try to stop all
            //foreach (var dev in devicesToStop) {
            //    var (success, msg) = StopDevice(dev, false);
            //    if (!success) {
            //        stopped = false;
            //        failReason = msg;
            //    }
            //}
            NiceHashStats.StateChanged();
            StopMining(true);
            return (stopped, failReason);
        }

        public static (bool stopped, string failReason) StopDevice(ComputeDevice device, bool refreshStateChange = true)
        {
            // we can only start a device it is already stopped
            switch (device.State)
            {
                case DeviceState.Stopped:
                    return (false, $"Device {device.Uuid} already stopped");
                case DeviceState.Benchmarking:
                    device.State = DeviceState.Stopped;
                    BenchmarkManager.StopBenchmarForDevice(device);
                    if (refreshStateChange) NiceHashStats.StateChanged();
                    return (true, "");
                case DeviceState.Mining:
                    device.State = DeviceState.Stopped;
                    UpdateDevicesToMine();
                    if (refreshStateChange) NiceHashStats.StateChanged();
                    return (true, "");
                default:
                    return (false, $"Cannot handle state {device.State.ToString()} for device {device.Uuid}");
            }
        }
    }
}
