using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Devices
{
    public static class AvailableDevices
    {
        private const string Tag = "AvailableDevices";

        private static readonly List<ComputeDevice> _devices = new List<ComputeDevice>();
        public static IReadOnlyList<ComputeDevice> Devices => _devices;

        public static bool HasNvidia => Devices.Any(d => d.DeviceType == DeviceType.NVIDIA);
        public static bool HasAmd => Devices.Any(d => d.DeviceType == DeviceType.AMD);
        public static bool HasCpu => Devices.Any(d => d.DeviceType == DeviceType.CPU);
        public static bool HasGpu => HasNvidia || HasAmd;

        public static int AvailCpus => GetCountForType(DeviceType.CPU);
        public static int AvailNVGpus => GetCountForType(DeviceType.NVIDIA);
        public static int AvailAmdGpus => GetCountForType(DeviceType.AMD);

        public static int AvailGpUs => AvailAmdGpus + AvailNVGpus;

        public static int NumDetectedNvDevs { get; internal set; }
        public static int NumDetectedAmdDevs { get; internal set; }

        public static int AmdOpenCLPlatformNum { get; internal set; } = -1;

        internal static void AddDevice(ComputeDevice dev)
        {
            _devices.Add(dev);
        }

        internal static void AddDevices(IEnumerable<ComputeDevice> devs)
        {
            _devices.AddRange(devs);
        }
        
        public static ComputeDevice GetDeviceWithUuid(string uuid)
        {
            return Devices.FirstOrDefault(dev => uuid == dev.Uuid);
        }

        public static List<ComputeDevice> GetSameDevicesTypeAsDeviceWithUuid(string uuid)
        {
            var compareDev = GetDeviceWithUuid(uuid);
            return (from dev in Devices
                    where uuid != dev.Uuid && compareDev.DeviceType == dev.DeviceType
                    select GetDeviceWithUuid(dev.Uuid)).ToList();
        }

        public static ComputeDevice GetCurrentlySelectedComputeDevice(int index, bool unique)
        {
            return Devices[index];
        }

        public static int GetCountForType(DeviceType type)
        {
            return Devices.Count(device => device.DeviceType == type);
        }

        internal static void UncheckCpuIfGpu()
        {
            if (!HasGpu) return;

            foreach (var dev in Devices.Where(d => d.DeviceType == DeviceType.CPU))
            {
                dev.Enabled = false;
            }
        }

        public static void RemoveInvalidDevs()
        {
            var invalidDevices = new List<ComputeDevice>();
            foreach (var cDev in Devices)
            {
                if (cDev.IsAlgorithmSettingsInitialized()) continue;

                Helpers.ConsolePrint(Tag, "CRITICAL ISSUE!!! Device has AlgorithmSettings == null. Will remove");
                invalidDevices.Add(cDev);
            }
            // remove invalids
            foreach (var invalid in invalidDevices)
            {
                _devices.Remove(invalid);
            }
        }
    }
}
