using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Mining.Benchmarking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        public static string CreateUsername(string btc, string rigID) => $"{btc}${rigID}";

        private static ConcurrentQueue<(ComputeDevice, DeviceState)> _scheduleUpdateDevicesToMineStates { get; set; } = new ConcurrentQueue<(ComputeDevice, DeviceState)>();

        private static void ScheduleStartMiningOnDevices(params ComputeDevice[] startDevices)
        {
            foreach (var startDevice in startDevices ?? Enumerable.Empty<ComputeDevice>())
            {
                startDevice.IsPendingChange = true;
                _scheduleUpdateDevicesToMineStates.Enqueue((startDevice, DeviceState.Mining));
            }
        }

        private static void ScheduleStopMiningOnDevices(params ComputeDevice[] stopDevices)
        {
            foreach (var stopDevice in stopDevices ?? Enumerable.Empty<ComputeDevice>())
            {
                stopDevice.IsPendingChange = true;
                _scheduleUpdateDevicesToMineStates.Enqueue((stopDevice, DeviceState.Stopped));
            }
        }

        private static async Task UpdateDevicesToMineTask()
        {
            // drain queue 
            var updateDeviceStates = new Dictionary<ComputeDevice, DeviceState>();
            while (_scheduleUpdateDevicesToMineStates.TryDequeue(out var pair))
            {
                var (device, state) = pair;
                updateDeviceStates[device] = state;
            }
            // and update states
            foreach (var newState in updateDeviceStates)
            {
                var device = newState.Key;
                var setState = newState.Value;
                device.State = setState; // THIS TRIGERS STATE CHANGE
            }
            var devicesToMine = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Mining).ToList();
            if (devicesToMine.Count > 0)
            {
                StartMining();
                await MiningManager.UpdateMiningSession(devicesToMine);
            }
            else
            {
                await StopMining();
            }
            // TODO implement and clear devicePending state changed
            foreach (var newState in updateDeviceStates)
            {
                var device = newState.Key;
                device.IsPendingChange = false;
            }
        }

        public static async Task<bool> StartSingleDevicePublic(ComputeDevice device)
        {
            if (device.IsPendingChange) return false;
            await StartDeviceTask(device);
            return true;
        }

        // TODO add check for any enabled algorithms
        public static async Task<(bool started, string failReason)> StartAllAvailableDevicesTask()
        {
            // TODO consider trying to start the error state devices as well
            var devicesToStart = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Stopped);
            if (devicesToStart.Count() == 0)
            {
                return (false, "there are no new devices to start");
            }

            // TODO for now no partial success so if one fails send back that everything fails
            var started = true;
            var failReason = "";
            var startTasks = new List<Task<(bool start, string failReason)>>();
            var startTasksAndMining = new List<Task>();
            foreach (var startDevice in devicesToStart)
            {
                // executeStart later in batch
                var startTask = StartDeviceTask(startDevice, false, false);
                startTasks.Add(startTask);
                startTasksAndMining.Add(startTask);
            }
            // finally add update devices to mine task
            startTasksAndMining.Add(UpdateDevicesToMineTask());
            // and await
            await Task.WhenAll(startTasksAndMining);

            // get task statuses
            var startDeviceTasks = devicesToStart.Zip(startTasks, (dev, task) => new { Device = dev, Task = task });
            foreach (var pair in startDeviceTasks)
            {
                var (deviceStarted, deviceFailReason) = pair.Task.Result;
                started &= deviceStarted;
                if (!deviceStarted)
                {
                    failReason += $"{pair.Device.Name} {deviceFailReason};";
                }
            }
            return (started, failReason);
        }

        internal static async Task<(bool started, string failReason)> StartDeviceTask(ComputeDevice device, bool skipBenchmark = false, bool executeStart = true)
        {
            device.StartState = true;
            // we can only start a device it is already stopped
            if (device.State == DeviceState.Disabled)
            {
                return (false, "Device is disabled");
            }

            if (device.State != DeviceState.Stopped && device.State != DeviceState.Error && !skipBenchmark)
            {
                return (false, "Device already started");
            }

            var started = true;
            var failReason = "";
            var allAlgorithmsDisabled = !device.AnyAlgorithmEnabled();
            var isAllZeroPayingState = device.AllEnabledAlgorithmsZeroPaying();
            // check if device has any benchmakrs
            var needBenchmarkOrRebench = device.AnyEnabledAlgorithmsNeedBenchmarking();
            if (allAlgorithmsDisabled)
            {
                device.State = DeviceState.Error;
                started = false;
                failReason = "Cannot start a device with all disabled algoirhtms";
            }
            else if (needBenchmarkOrRebench && !skipBenchmark)
            {
                BenchmarkManager.StartBenchmarForDevice(device, new BenchmarkStartSettings
                {
                    StartMiningAfterBenchmark = true,
                    BenchmarkPerformanceType = BenchmarkPerformanceType.Standard,
                    BenchmarkOption = BenchmarkOption.ZeroOrReBenchOnly
                });
            }
            else if (isAllZeroPayingState)
            {
                device.State = DeviceState.Error;
                started = false;
                failReason = "No enabled algorithm is profitable";
            }
            else
            {
                ScheduleStartMiningOnDevices(device);
                if (executeStart)
                {
                    await UpdateDevicesToMineTask();
                }
            }

            return (started, failReason);
        }

        public static async Task<bool> StopSingleDevicePublic(ComputeDevice device)
        {
            if (device.IsPendingChange) return false;
            await StopDeviceTask(device);
            return true;
        }

        public static async Task<(bool stopped, string failReason)> StopAllDevicesTask()
        {
            // TODO when starting and stopping we are not taking Pending and Error states into account
            var devicesToStop = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking);
            if (devicesToStop.Count() == 0)
            {
                return (false, "No new devices to stop");
            }

            var anyMining = devicesToStop.Any(dev => dev.State == DeviceState.Mining);
            var stopped = true;
            var failReason = "";
            var stopTasks = new List<Task<(bool stopped, string failReason)>>();
            foreach (var stopDevice in devicesToStop)
            {
                stopTasks.Add(StopDeviceTask(stopDevice, false));
            }
            // TODO simplify this after MiningManager refactor
            if (anyMining)
            {
                var stopTasksAndMining = new List<Task>();
                stopTasksAndMining.Add(UpdateDevicesToMineTask());
                foreach (var t in stopTasks) stopTasksAndMining.Add(t);
                await Task.WhenAll(stopTasksAndMining);
            }
            else
            {
                await Task.WhenAll(stopTasks);
            }

            var stopedDeviceTasks = devicesToStop.Zip(stopTasks, (dev, task) => new { Device = dev, Task = task });
            foreach (var pair in stopedDeviceTasks)
            {
                var (deviceStopped, deviceFailReason) = pair.Task.Result;
                stopped &= deviceStopped;
                if (!deviceStopped)
                {
                    failReason += $"{pair.Device.Name} {deviceFailReason};";
                }
            }
            return (stopped, failReason);
        }

        internal static async Task<(bool stopped, string failReason)> StopDeviceTask(ComputeDevice device, bool executeStop = true)
        {
            device.StartState = false;
            // we can only stop a device it is mining or benchmarking
            switch (device.State)
            {
                case DeviceState.Stopped:
                    return (false, $"Device {device.Uuid} already stopped");
                case DeviceState.Benchmarking:
                    await BenchmarkManager.StopBenchmarForDevice(device); // TODO benchmarking is in a Task
                    return (true, "");
                case DeviceState.Mining:
                    ScheduleStopMiningOnDevices(device);
                    if (executeStop)
                    {
                        await UpdateDevicesToMineTask();
                    }
                    return (true, "");
                default:
                    return (false, $"Cannot handle state {device.State} for device {device.Uuid}");
            }
        }

        // TODO make this smarter 
        // TODO this is after we are finished with miner plugin install/update/remove
        internal static async Task RestartDevicesState()
        {
            var stopTasks = new List<Task>();
            var devicesToStart = new List<ComputeDevice>();
            var devicesToBenchmark = new List<ComputeDevice>();
            foreach (var dev in AvailableDevices.Devices)
            {
                if (dev.StartState)
                {
                    devicesToStart.Add(dev);
                }
                else if (dev.State == DeviceState.Benchmarking)
                {
                    devicesToBenchmark.Add(dev); // TODO should we start benchmarks without starting mining afterwards???
                }
                stopTasks.Add(StopDeviceTask(dev, false));
            }
            stopTasks.Add(UpdateDevicesToMineTask());
            await Task.WhenAll(stopTasks);

            foreach (var benchDev in devicesToBenchmark)
            {
                BenchmarkManager.StartBenchmarForDevice(benchDev, new BenchmarkStartSettings
                {
                    StartMiningAfterBenchmark = true, // TODO should we start mining after benchmark?
                    BenchmarkPerformanceType = BenchmarkManagerState.Instance.SelectedBenchmarkType,
                    BenchmarkOption = BenchmarkOption.ZeroOrReBenchOnly
                });
            }

            var startTasks = new List<Task>();
            // now attempt restart 
            foreach (var dev in devicesToStart)
            {
                startTasks.Add(StartDeviceTask(dev, false, false));
            }
            startTasks.Add(UpdateDevicesToMineTask());
            await Task.WhenAll(startTasks);
        }

        // Check all devices that have (re)benchmarks
        public static void StartBenchmark()
        {
            var devices = AvailableDevices.Devices.Where(device => device.AnyEnabledAlgorithmsNeedBenchmarking() && device.State == DeviceState.Stopped);
            foreach (var device in devices)
            {
                BenchmarkManager.StartBenchmarForDevice(device, new BenchmarkStartSettings
                {
                    StartMiningAfterBenchmark = true, // TODO should we start mining after benchmark?
                    BenchmarkPerformanceType = BenchmarkManagerState.Instance.SelectedBenchmarkType,
                    BenchmarkOption = BenchmarkOption.ZeroOrReBenchOnly
                });
            }
        }

        public static Task StopBenchmark()
        {
            return BenchmarkManager.Stop();
        }

        #region Updater mining state save/restore
        private static string _miningStateFilePath => Paths.InternalsPath("DeviceRestoreStates.json");
        private struct DeviceRestoreState
        {
            public bool IsStarted { get; set; }
            public DeviceState LastState { get; set; }
        }
        internal static void SaveMiningState()
        {
            var devicesRestoreStates = new Dictionary<string, DeviceRestoreState>();
            foreach (var dev in AvailableDevices.Devices)
            {
                devicesRestoreStates[dev.Uuid] = new DeviceRestoreState { IsStarted = dev.StartState, LastState = dev.State };
            }
            InternalConfigs.WriteFileSettings(_miningStateFilePath, devicesRestoreStates);
        }

        internal static async Task RestoreMiningState()
        {
            var devicesRestoreStates = InternalConfigs.ReadFileSettings<Dictionary<string, DeviceRestoreState>>(_miningStateFilePath);
            if (devicesRestoreStates == null)
            {
                var miningStateFilePathOLD = Paths.RootPath("DeviceRestoreStates.json");
                // check the old path since older versions will save it there
                devicesRestoreStates = InternalConfigs.ReadFileSettings<Dictionary<string, DeviceRestoreState>>(miningStateFilePathOLD);
                try
                {
                    File.Delete(miningStateFilePathOLD);
                }
                catch (Exception e)
                {
                    Logger.Error("ApplicationStateManager.Mining", $"RestoreMiningState delete old Exception: {e.Message}");
                }
                if (devicesRestoreStates == null) return;
            }
            try
            {
                File.Delete(_miningStateFilePath);
            }
            catch (Exception e)
            {
                Logger.Error("ApplicationStateManager.Mining", $"RestoreMiningState Exception: {e.Message}");
            }
            // restore states
            var startTasks = new List<Task>();
            // now attempt restart 
            foreach (var devState in devicesRestoreStates)
            {
                var dev = AvailableDevices.GetDeviceWithUuid(devState.Key);
                var shouldStart = devState.Value.IsStarted || devState.Value.LastState == DeviceState.Benchmarking || devState.Value.LastState == DeviceState.Mining;
                if (dev == null || shouldStart == false) continue;
                startTasks.Add(StartDeviceTask(dev, false, false));
            }
            startTasks.Add(UpdateDevicesToMineTask());
            await Task.WhenAll(startTasks);
        }

        #endregion Update state push/pop
    }
}
