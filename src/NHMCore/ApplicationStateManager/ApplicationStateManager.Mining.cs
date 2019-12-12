using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Mining.Benchmarking;
using NHMCore.Configs;
using NHMCore.Mining;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        public static string GetUsername()
        {
            if (MiningState.Instance.IsDemoMining || !CredentialsSettings.Instance.IsBitcoinAddressValid) {
                return DemoUser.BTC;
            }
            var btc = CredentialsSettings.Instance.BitcoinAddress.Trim();
            return $"{btc}${RigID}";
        }

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
            if (devicesToMine.Count > 0) {
                StartMining();
                await MiningManager.UpdateMiningSession(devicesToMine, GetUsername());
            } else {
                await StopMining();
            }
            // TODO implement and clear devicePending state changed
            foreach (var newState in updateDeviceStates)
            {
                var device = newState.Key;
                device.IsPendingChange = false;
            }
        }

        private static async Task RestartMinersIfMining()
        {
            // if mining update the mining manager
            if (MiningState.Instance.IsCurrentlyMining)
            {
                await MiningManager.RestartMiners(GetUsername());
            }
        }

        #region CHECK TO DELETE
        public static void ResumeMiners()
        {
            if (_resumeOldState)
            {
                _resumeOldState = false;
                foreach (var dev in _resumeDevs)
                {
                    StartDeviceTask(dev);
                }
                _resumeDevs.Clear();
            }
            else
            {
                // TODO here we probably don't care to wait the Task to complete
                _ = RestartMinersIfMining();
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
                    StopAllDevicesTask();
                }
            }
        }
        #endregion CHECK TO DELETE


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
            if (devicesToStart.Count() == 0) {
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
            // we can only start a device it is already stopped
            if (device.State == DeviceState.Disabled)
            {
                return (false, "Device is disabled");
            }

            if (device.State != DeviceState.Stopped && !skipBenchmark)
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

        public static async Task<(bool stopped, string failReason)> StopAllDevicesTask() {
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
                    return (false, $"Cannot handle state {device.State.ToString()} for device {device.Uuid}");
            }
        }
    }
}
