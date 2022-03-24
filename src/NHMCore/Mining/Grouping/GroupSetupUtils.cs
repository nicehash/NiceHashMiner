using NHM.Common;
using NHM.Common.Enums;
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

        private static (ComputeDevice device, DeviceMiningStatus status) GetDeviceMiningStatus(ComputeDevice device)
        {
            if (device == null) return (device, DeviceMiningStatus.DeviceNull);
            if (device.IsDisabled) return (device, DeviceMiningStatus.Disabled);
            var hasEnabledAlgo = device.AlgorithmSettings.Any(IsAlgoMiningCapable);
            if (hasEnabledAlgo == false) return (device, DeviceMiningStatus.NoEnabledAlgorithms);
            return (device, DeviceMiningStatus.CanMine);
        }

        private static string GetDisabledDeviceStatusString(ComputeDevice dev, DeviceMiningStatus status)
        {
            switch (status)
            {
                case DeviceMiningStatus.DeviceNull:
                    return "Passed Device is NULL";
                case DeviceMiningStatus.Disabled:
                    return "DISABLED: " + dev.GetFullName();
                case DeviceMiningStatus.NoEnabledAlgorithms:
                    return "No Enabled Algorithms: " + dev.GetFullName();
                default: return "Invalid status Passed";
            }            
        }

        private static void LogMiningNonMiningStatuses(IEnumerable<(ComputeDevice device, DeviceMiningStatus status)> devicesMiningStatusPairs)
        {
            var nonMiningDevStatuses = devicesMiningStatusPairs
                .Where((devStatus) => devStatus.status != DeviceMiningStatus.CanMine)
                .ToArray();
            var miningDevices = devicesMiningStatusPairs
                .Where((devStatus) => devStatus.status == DeviceMiningStatus.CanMine)
                .Select((devStatus) => new MiningDevice(devStatus.device))
                .ToArray();

            // print statuses
            if (nonMiningDevStatuses.Length > 0)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("Disabled Devices:");
                foreach (var (dev, status) in nonMiningDevStatuses)
                {
                    stringBuilder.AppendLine("\t" + GetDisabledDeviceStatusString(dev, status));
                }
                Logger.Info(Tag, stringBuilder.ToString());
            }

            if (miningDevices.Length > 0)
            {
                // print enabled
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("Enabled Devices for Mining session:");
                foreach (var miningDevice in miningDevices)
                {
                    var device = miningDevice.Device;
                    stringBuilder.AppendLine($"\tENABLED ({device.GetFullName()})");
                    foreach (var algo in device.AlgorithmSettings)
                    {
                        var isEnabled = IsAlgoMiningCapable(algo);
                        stringBuilder.AppendLine($"\t\tALGORITHM {(isEnabled ? "ENABLED " : "DISABLED")} ({algo.AlgorithmStringID})");
                    }
                }
                Logger.Info(Tag, stringBuilder.ToString());
            }
        }

        public static List<MiningDevice> GetMiningDevices(IEnumerable<ComputeDevice> devices, bool log)
        {
            if (BuildOptions.FORCE_MINING) return devices.Select(dev => new MiningDevice(dev)).ToList();

            var devicesMiningStatusPairs = devices.Select(GetDeviceMiningStatus).ToArray();
            if (log) LogMiningNonMiningStatuses(devicesMiningStatusPairs);

            var miningDevices = devicesMiningStatusPairs
                .Where((devStatus) => devStatus.status == DeviceMiningStatus.CanMine)
                .Select((devStatus) => new MiningDevice(devStatus.device))
                .ToList();

            return miningDevices;
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

