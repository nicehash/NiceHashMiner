using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Grouping
{
    public static class GroupSetupUtils
    {
        private const string Tag = "GroupSetupUtils";

        public static bool IsAlgoMiningCapable(Algorithm algo)
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
            else if (device.Enabled == false)
            {
                status = DeviceMiningStatus.Disabled;
            }
            else
            {
                var hasEnabledAlgo = device.GetAlgorithmSettings().Aggregate(false,
                    (current, algo) =>
                        current | (IsAlgoMiningCapable(algo) && MinerPaths.IsValidMinerPath(algo.MinerBinaryPath)));
                if (hasEnabledAlgo == false)
                {
                    status = DeviceMiningStatus.NoEnabledAlgorithms;
                }
            }

            return new Tuple<ComputeDevice, DeviceMiningStatus>(device, status);
        }

        private static Tuple<List<MiningDevice>, List<Tuple<ComputeDevice, DeviceMiningStatus>>>
            GetMiningAndNonMiningDevices(List<ComputeDevice> devices)
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

                Helpers.ConsolePrint(Tag, stringBuilder.ToString());
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
                    foreach (var algo in device.GetAlgorithmSettings())
                    {
                        var isEnabled = IsAlgoMiningCapable(algo) && MinerPaths.IsValidMinerPath(algo.MinerBinaryPath);
                        stringBuilder.AppendLine(
                            $"\t\tALGORITHM {(isEnabled ? "ENABLED " : "DISABLED")} ({algo.AlgorithmStringID})");
                    }
                }

                Helpers.ConsolePrint(Tag, stringBuilder.ToString());
            }
        }

        public static List<MiningDevice> GetMiningDevices(List<ComputeDevice> devices, bool log)
        {
            var miningNonMiningDevs = GetMiningAndNonMiningDevices(devices);
            if (log)
            {
                LogMiningNonMiningStatuses(miningNonMiningDevs.Item1, miningNonMiningDevs.Item2);
            }

            return miningNonMiningDevs.Item1;
        }

        // avarage passed in benchmark values
        public static void AvarageSpeeds(List<MiningDevice> miningDevs)
        {
            // calculate avarage speeds, to ensure mining stability
            // device name, algo key, algos refs list
            var allAvaragers = new Dictionary<string, AveragerGroup>();

            // init empty avarager
            foreach (var device in miningDevs)
            {
                var devName = device.Device.Name;
                allAvaragers[devName] = new AveragerGroup();
            }

            // fill avarager
            foreach (var device in miningDevs)
            {
                var devName = device.Device.Name;
                // add UUID
                allAvaragers[devName].UuidList.Add(device.Device.Uuid);
                allAvaragers[devName].AddAlgorithms(device.Algorithms);
            }

            // calculate and set new AvarageSpeeds for miningDeviceReferences
            foreach (var curAvaragerKvp in allAvaragers)
            {
                var curAvarager = curAvaragerKvp.Value;
                var calculatedAvaragers = curAvarager.CalculateAvarages();
                foreach (var uuid in curAvarager.UuidList)
                {
                    var minerDevIndex = miningDevs.FindIndex((dev) => dev.Device.Uuid == uuid);
                    if (minerDevIndex > -1)
                    {
                        foreach (var avgKvp in calculatedAvaragers)
                        {
                            var algoID = avgKvp.Key;
                            var avaragedSpeed = avgKvp.Value[0];
                            var secondaryAveragedSpeed = avgKvp.Value[1];
                            var index = miningDevs[minerDevIndex].Algorithms
                                .FindIndex((a) => a.AlgorithmStringID == algoID);
                            if (index > -1)
                            {
                                miningDevs[minerDevIndex].Algorithms[index].AvaragedSpeed = avaragedSpeed;
                                if (miningDevs[minerDevIndex].Algorithms[index] is DualAlgorithm dualAlgo)
                                {
                                    dualAlgo.SecondaryAveragedSpeed = secondaryAveragedSpeed;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class SpeedSumCount
    {
        public double Speed = 0;
        public double SecondarySpeed = 0;
        public int Count = 0;

        public double GetAvarage()
        {
            if (Count > 0)
            {
                return Speed / Count;
            }
            return 0;
        }

        public double GetSecondaryAverage()
        {
            if (Count > 0)
            {
                return SecondarySpeed / Count;
            }
            return 0;
        }
    }

    public class AveragerGroup
    {
        public string DeviceName;

        public List<string> UuidList = new List<string>();

        // algo_id, speed_sum, speed_count
        public Dictionary<string, SpeedSumCount> BenchmarkSums = new Dictionary<string, SpeedSumCount>();

        public Dictionary<string, List<double>> CalculateAvarages()
        {
            var ret = new Dictionary<string, List<double>>();
            foreach (var kvp in BenchmarkSums)
            {
                var algoID = kvp.Key;
                var ssc = kvp.Value;
                ret[algoID] = new List<double>
                {
                    ssc.GetAvarage(),
                    ssc.GetSecondaryAverage()
                };
            }
            return ret;
        }

        public void AddAlgorithms(List<Algorithm> algos)
        {
            foreach (var algo in algos)
            {
                var algoID = algo.AlgorithmStringID;
                double secondarySpeed = 0;
                if (algo is DualAlgorithm dualAlgo)
                {
                    secondarySpeed = dualAlgo.SecondaryBenchmarkSpeed;
                }
                if (BenchmarkSums.ContainsKey(algoID) == false)
                {
                    var ssc = new SpeedSumCount
                    {
                        Count = 1,
                        Speed = algo.BenchmarkSpeed,
                        SecondarySpeed = secondarySpeed
                    };
                    BenchmarkSums[algoID] = ssc;
                }
                else
                {
                    BenchmarkSums[algoID].Count++;
                    BenchmarkSums[algoID].Speed += algo.BenchmarkSpeed;
                    BenchmarkSums[algoID].SecondarySpeed += secondarySpeed;
                }
            }
        }
    }
}
