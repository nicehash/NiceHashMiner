using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Benchmarking;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        public static bool IsDemoMining { get; set; } = false;

        public static string GetUsername()
        {
            if (IsDemoMining) {
                return DemoUser.BTC;
            }

            return Globals.GetUsername();
        }

        private static void UpdateDevicesToMine()
        {
            var allDevs = AvailableDevices.Devices;
            var devicesToMine = allDevs.Where(dev => dev.State == DeviceState.Mining).ToList();
            if (devicesToMine.Count > 0) {
                StartMining();
                MinersManager.EnsureMiningSession(GetUsername());
                MinersManager.UpdateUsedDevices(devicesToMine);
            } else {
                StopMining(false);
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
            var devicesToBenchmark = devicesToStart.Where(dev => BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(dev));
            foreach (var dev in devicesToBenchmark) {
               dev.State = DeviceState.Benchmarking;
               BenchmarkManager.StartBenchmarForDevice(dev, true);
            }
            
            // TODO check count
            var devicesToMine = devicesToStart.Where(dev => !BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(dev)).ToList();
            foreach (var dev in devicesToMine) {
                dev.State = DeviceState.Mining;
            }
            UpdateDevicesToMine();
            if (!isRpcCall) {
                NiceHashStats.StateChanged();
            }
            ToggleActiveInactiveDisplay();
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

            // check if device has any benchmakrs
            var needsBenchmark = BenchmarkChecker.IsDeviceWithAllEnabledAlgorithmsWithoutBenchmarks(device);
            if (needsBenchmark && !skipBenhcmakrk)
            {
                device.State = DeviceState.Benchmarking;
                BenchmarkManager.StartBenchmarForDevice(device, true);
            }
            else
            {
                device.State = DeviceState.Mining;
                UpdateDevicesToMine();
            }

            ToggleActiveInactiveDisplay();
            RefreshDeviceListView?.Invoke(null, null);
            NiceHashStats.StateChanged();

            return (true, "");
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
            ToggleActiveInactiveDisplay();
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
                    ToggleActiveInactiveDisplay();
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
