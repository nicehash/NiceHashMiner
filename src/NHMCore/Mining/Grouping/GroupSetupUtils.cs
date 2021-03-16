using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHMCore.Mining.Grouping
{
    public static class GroupSetupUtils
    {
        private const string Tag = "GroupSetupUtils";

        public static bool IsAlgoMiningCapable(AlgorithmContainer algo)
        {
            return algo != null && algo.Enabled && algo.BenchmarkSpeed > 0;
        }

        public static Tuple<ComputeDevice, DeviceMiningStatus> GetDeviceMiningStatus(ComputeDevice device)
        {
            var status = DeviceMiningStatus.CanMine;
            if (device == null)
            {
                // C# is null happy
                status = DeviceMiningStatus.DeviceNull;
            }
            else if (device.IsDisabled)
            {
                status = DeviceMiningStatus.Disabled;
            }
            else
            {
                var hasEnabledAlgo = device.AlgorithmSettings.Any((algo) => IsAlgoMiningCapable(algo));
                if (hasEnabledAlgo == false)
                {
                    status = DeviceMiningStatus.NoEnabledAlgorithms;
                }
            }

            return new Tuple<ComputeDevice, DeviceMiningStatus>(device, status);
        }

        private static Tuple<List<MiningDevice>, List<Tuple<ComputeDevice, DeviceMiningStatus>>>
            GetMiningAndNonMiningDevices(IEnumerable<ComputeDevice> devices)
        {
            var nonMiningDevStatuses = new List<Tuple<ComputeDevice, DeviceMiningStatus>>();
            var miningDevices = new List<MiningDevice>();
            foreach (var dev in devices)
            {
                var devStatus = GetDeviceMiningStatus(dev);
                if (devStatus.Item2 == DeviceMiningStatus.CanMine)
                {
                    miningDevices.Add(new MiningDevice(dev));
                }
                else
                {
                    nonMiningDevStatuses.Add(devStatus);
                }
            }

            return new Tuple<List<MiningDevice>, List<Tuple<ComputeDevice, DeviceMiningStatus>>>(miningDevices,
                nonMiningDevStatuses);
        }

        private static string GetDisabledDeviceStatusString(Tuple<ComputeDevice, DeviceMiningStatus> devStatusTuple)
        {
            var dev = devStatusTuple.Item1;
            var status = devStatusTuple.Item2;
            switch (status)
            {
                case DeviceMiningStatus.DeviceNull:
                    return "Passed Device is NULL";
                case DeviceMiningStatus.Disabled:
                    return "DISABLED: " + dev.GetFullName();
                case DeviceMiningStatus.NoEnabledAlgorithms:
                    return "No Enabled Algorithms: " + dev.GetFullName();
            }

            return "Invalid status Passed";
        }

        private static void LogMiningNonMiningStatuses(List<MiningDevice> enabledDevices,
            List<Tuple<ComputeDevice, DeviceMiningStatus>> disabledDevicesStatuses)
        {
            // print statuses
            if (disabledDevicesStatuses.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("Disabled Devices:");
                foreach (var deviceStatus in disabledDevicesStatuses)
                {
                    stringBuilder.AppendLine("\t" + GetDisabledDeviceStatusString(deviceStatus));
                }

                Logger.Info(Tag, stringBuilder.ToString());
            }

            if (enabledDevices.Count > 0)
            {
                // print enabled
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("Enabled Devices for Mining session:");
                foreach (var miningDevice in enabledDevices)
                {
                    var device = miningDevice.Device;
                    stringBuilder.AppendLine($"\tENABLED ({device.GetFullName()})");
                    foreach (var algo in device.AlgorithmSettings)
                    {
                        var isEnabled = IsAlgoMiningCapable(algo);
                        stringBuilder.AppendLine(
                            $"\t\tALGORITHM {(isEnabled ? "ENABLED " : "DISABLED")} ({algo.AlgorithmStringID})");
                    }
                }

                Logger.Info(Tag, stringBuilder.ToString());
            }
        }

        public static List<MiningDevice> GetMiningDevices(IEnumerable<ComputeDevice> devices, bool log)
        {
            if (BuildOptions.FORCE_MINING)
            {
                return devices.Select(dev => new MiningDevice(dev)).ToList(); ;
            }

            var miningNonMiningDevs = GetMiningAndNonMiningDevices(devices);
            if (log)
            {
                LogMiningNonMiningStatuses(miningNonMiningDevs.Item1, miningNonMiningDevs.Item2);
            }

            return miningNonMiningDevs.Item1;
        }

        // TODO we can now add by device name device type or whatever avarage passed in benchmark values
        public static void AvarageSpeeds(IEnumerable<MiningDevice> miningDevs)
        {
            if (miningDevs == null || !miningDevs.Any()) return; 
            // calculate avarage speeds, to ensure mining stability
            // device name, algo key, algos refs list
            var groupedAlgorithmsAll = miningDevs
                .GroupBy(md => md.Device.Name)
                .SelectMany(devsByName => devsByName.SelectMany(md => md.Algorithms)
                    .GroupBy(algo => algo.AlgorithmStringID)
                    .Select(g => g.Select(setAlgo => setAlgo).ToArray()))
                .Where(groupedAlgorithms => groupedAlgorithms.Length > 0)
                .ToArray(); 
            foreach (var groupedAlgorithms in groupedAlgorithmsAll)
            {
                // keep BenchmarkSpeed because of the dev features get rid of those
                var avgSpeeds = groupedAlgorithms.Select(algo => new double[] { algo.BenchmarkSpeed, algo.SecondaryBenchmarkSpeed })
                .Aggregate(new double[] { 0.0, 0.0 }, (total, next) => total.Zip(next, (sum, plus) => sum + plus).ToArray())
                .Select(sumSpeed => sumSpeed / groupedAlgorithms.Length)
                .ToArray();
                foreach (var algo in groupedAlgorithms)
                {
                    for (int i = 0; i < avgSpeeds.Length; i++) algo.AveragedSpeeds[i] = avgSpeeds[i];
                }
            }
        }
    }
}

