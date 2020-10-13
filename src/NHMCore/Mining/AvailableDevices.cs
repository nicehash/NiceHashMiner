using NHM.Common.Device;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Mining
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

        public static int AvailGpus => AvailAmdGpus + AvailNVGpus;


        public static ulong AvailNvidiaGpuRam
        {
            get
            {
                var ramSum = 0ul;
                var gpuRams = _devices
                    .Where(dev => dev.BaseDevice is CUDADevice)
                    .Select(dev => dev.BaseDevice)
                    .Cast<CUDADevice>()
                    .Select(gpu => gpu.GpuRam);
                foreach (var ram in gpuRams) ramSum += ram;
                return ramSum;
            }
        }

        public static ulong AvailAmdGpuRam
        {
            get
            {
                var ramSum = 0ul;
                var gpuRams = _devices
                    .Where(dev => dev.BaseDevice is AMDDevice)
                    .Select(dev => dev.BaseDevice)
                    .Cast<AMDDevice>()
                    .Select(gpu => gpu.GpuRam);
                foreach (var ram in gpuRams) ramSum += ram;
                return ramSum;
            }
        }

        public static int NumDetectedNvDevs => _devices.Count(d => d.DeviceType == DeviceType.NVIDIA);
        public static int NumDetectedAmdDevs => _devices.Count(d => d.DeviceType == DeviceType.AMD);

        internal static void AddDevice(ComputeDevice dev)
        {
            _devices.Add(dev);
        }

        internal static bool IsEnableAllDevicesRedundantOperation()
        {
            var allEnabled = Devices.All(dev => !dev.IsDisabled);
            return allEnabled;
        }

        internal static bool IsDisableAllDevicesRedundantOperation()
        {
            return !IsEnableAllDevicesRedundantOperation();
        }

        public static ComputeDevice GetDeviceWithUuid(string uuid)
        {
            return Devices.FirstOrDefault(dev => uuid == dev.Uuid);
        }

        public static ComputeDevice GetDeviceWithUuidOrB64Uuid(string uuid)
        {
            return Devices.FirstOrDefault(dev => uuid == dev.Uuid || uuid == dev.B64Uuid);
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
                if (dev.BaseDevice is CPUDevice cpu && cpu.CpuID.IsZen == false) dev.Enabled = false;
            }
        }
    }
}
