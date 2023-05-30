using NHM.Common;
using NHM.Common.Configs;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Nhmws;
using NHMCore.Notifications;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        public static string CreateUsername(string btc, string rigID) => $"{btc}${rigID}";

        // TODO add check for any enabled algorithms
        public static async Task<(bool started, string failReason)> StartAllAvailableDevicesTask()
        {
            EventManager.Instance.AddEventRigStarted(true);
            MiningState.Instance.MiningStoppedByToggle = false;
            // TODO consider trying to start the error state devices as well
            var devicesToStart = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Stopped);
            if (devicesToStart.Count() == 0)
            {
                return (false, "there are no new devices to start");
            }

            // TODO for now no partial success so if one fails send back that everything fails
            var started = true;
            var failReason = "";
            var startTasks = devicesToStart.Select(startDevice => StartDeviceTask(startDevice)).ToArray();
            // and await
            await Task.WhenAll(startTasks);

            // get task statuses
            var startDeviceTasks = devicesToStart.Zip(startTasks, (dev, task) => (device: dev, task.Result));
            foreach (var (device, result) in startDeviceTasks)
            {
                var (deviceStarted, deviceFailReason) = result;
                started &= deviceStarted;
                if (!deviceStarted) failReason += $"{device.Name} {deviceFailReason};";
            }
            return (started, failReason);
        }

        internal static async Task<(bool started, string failReason)> StartDeviceTask(ComputeDevice device)
        {
            device.StartState = true;
            // we can only start a device it is already stopped
            if (device.State == DeviceState.Disabled)
            {
                return (false, "Device is disabled");
            }

            if (device.State != DeviceState.Stopped && device.State != DeviceState.Error)
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
            else if (isAllZeroPayingState && !needBenchmarkOrRebench)
            {
                device.State = DeviceState.Error;
                started = false;
                failReason = "No enabled algorithm is profitable";
            }
            else
            {
                await MiningManager.StartDevice(device);
            }

            return (started, failReason);
        }

        public static async Task<(bool stopped, string failReason)> StopAllDevicesTask()
        {
            EventManager.Instance.AddEventRigStopped(true);
            // TODO when starting and stopping we are not taking Pending and Error states into account
#if NHMWS4
            var devicesToStop = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking || dev.State == DeviceState.Testing);
#else
            var devicesToStop = AvailableDevices.Devices.Where(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Benchmarking);
#endif
            if (devicesToStop.Count() == 0)
            {
                return (false, "No new devices to stop");
            }

            var stopped = true;
            var failReason = "";
            var stopTasks = devicesToStop.Select(StopDeviceTask).ToArray();
            await Task.WhenAll(stopTasks);

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

        internal static async Task<(bool stopped, string failReason)> StopDeviceTask(ComputeDevice device)
        {
            device.StartState = false;
            // we can only stop a device it is mining or benchmarking
            switch (device.State)
            {
                case DeviceState.Stopped:
                    return (false, $"Device {device.Uuid} already stopped");
                case DeviceState.Mining:
                case DeviceState.Benchmarking:
#if NHMWS4
                case DeviceState.Testing:
                    if (Helpers.IsElevated)
                    {
                        device.ResetFanSpeed();
                        device.ResetCoreClock();
                        device.ResetCoreClockDelta();
                        device.ResetMemoryClock();
                        device.ResetMemoryClockDelta();
                        device.ResetCoreVoltage();
                    }
#endif
                    await MiningManager.StopDevice(device);
                    return (true, "");
                default:
                    return (false, $"Cannot handle state {device.State} for device {device.Uuid}");
            }
        }

        // call this is after we are finished with miner plugin install/update/remove
        internal static async Task RestartDevicesState()
        {
            // restart all started 
            var devsToRestart = AvailableDevices.Devices
                .Where(dev => dev.StartState)
                .Select(MiningManager.StartDevice)
                .ToArray();
            await Task.WhenAll(devsToRestart);
        }

        // Check all devices that have (re)benchmarks
        public static void StartBenchmark()
        {
            var startBenchmarkingDevices = AvailableDevices.Devices
                .Where(device => device.State == DeviceState.Stopped)
                .Where(device => device.AnyEnabledAlgorithmsNeedBenchmarking())
                .Select(StartDeviceTask)
                .ToArray();
            _ = Task.WhenAll(startBenchmarkingDevices);
        }
        public static Task<(ErrorCode err, string msg)> StartReBenchmark()
        {
            //check if any exist
            var startBenchmarkingDevices = AvailableDevices.Devices
                .Where(device => device.State == DeviceState.Stopped)?
                .Where(device => device.AnyAlgorithmEnabled());
            if(startBenchmarkingDevices == null || startBenchmarkingDevices.Count() == 0)
            {
                return Task.FromResult((ErrorCode.ErrNoAlgoDataFound, "No targets found. Stop mining first."));
            }
            foreach (var device in startBenchmarkingDevices) device.PrepareForRebenchmark();
            var completeBenchmarkDevices = startBenchmarkingDevices
                .Select(StartDeviceTask)
                .ToArray();
            _ = Task.WhenAll(completeBenchmarkDevices);
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public static Task<(ErrorCode err, string msg)> StartRebenchmarkSpecific(string deviceUUID)
        {
            var startBenchmarkingDevices = AvailableDevices.Devices
                .Where(device => device.B64Uuid == deviceUUID)?
                .Where(device => device.State == DeviceState.Stopped)?
                .Where(device => device.AnyAlgorithmEnabled());
            if (startBenchmarkingDevices == null || startBenchmarkingDevices.Count() == 0)
            {
                return Task.FromResult((ErrorCode.ErrNoAlgoDataFound, "No targets found. Stop mining first."));
            }
            foreach (var device in startBenchmarkingDevices) device.PrepareForRebenchmark();
            var completeBenchmarkDevices = startBenchmarkingDevices
                .Select(StartDeviceTask)
                .ToArray();
            _ = Task.WhenAll(completeBenchmarkDevices);
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public static Task StopBenchmark()
        {
            var stoptDevices = AvailableDevices.Devices
                .Where(device => device.State == DeviceState.Benchmarking)
                .Select(StopDeviceTask)
                .ToArray();
            return Task.WhenAll(stoptDevices);
        }

#region Updater mining state save/restore
        private static string _miningStateFilePath => Paths.InternalsPath("DeviceRestoreStates.json");
        private struct DeviceRestoreState
        {
            public bool IsStarted { get; set; }
            public DeviceState LastState { get; set; }
#if NHMWS4
            public bool ShouldStart() => IsStarted || LastState == DeviceState.Benchmarking || LastState == DeviceState.Mining || LastState == DeviceState.Testing;
#else
            public bool ShouldStart() => IsStarted || LastState == DeviceState.Benchmarking || LastState == DeviceState.Mining;
#endif
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
            bool oldPath = false;
            var miningStateFilePathOLD = Paths.RootPath("DeviceRestoreStates.json");
            if (devicesRestoreStates == null)
            {
                // check the old path since older versions will save it there
                devicesRestoreStates = InternalConfigs.ReadFileSettings<Dictionary<string, DeviceRestoreState>>(miningStateFilePathOLD);
                oldPath = devicesRestoreStates != null;
            }
            if (devicesRestoreStates == null) return;
            try
            {
                File.Delete(oldPath ? miningStateFilePathOLD : _miningStateFilePath);
            }
            catch (Exception e)
            {
                var oldPathStr = oldPath ? "old path" : "";
                Logger.Error("ApplicationStateManager.Mining", $"RestoreMiningState delete {oldPathStr} Exception: {e.Message}");
            }
            // restore states
            var startTasks = devicesRestoreStates.Where(devStatePair => devStatePair.Value.ShouldStart())
                .Select(devStatePair => AvailableDevices.GetDeviceWithUuid(devStatePair.Key))
                .Where(dev => dev != null)
                .Select(StartDeviceTask)
                .ToArray();
            // now attempt restart
            await Task.WhenAll(startTasks);
        }

#endregion Update state push/pop
    }
}
