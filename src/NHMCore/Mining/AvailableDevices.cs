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

        private static readonly List<ComputeDevice> _gpus = new List<ComputeDevice>();
        public static IReadOnlyList<ComputeDevice> GPUs => _gpus;

        public static bool HasNvidia => Devices.Any(d => d.DeviceType == DeviceType.NVIDIA);
        public static bool HasAmd => Devices.Any(d => d.DeviceType == DeviceType.AMD);
        public static bool HasCpu => Devices.Any(d => d.DeviceType == DeviceType.CPU);
        public static bool HasGpu => HasNvidia || HasAmd;
        public static bool HasGpuToPause => Devices.Any(dev => dev.PauseMiningWhenGamingMode && dev.DeviceType != DeviceType.CPU);
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
                    .Cast<IGpuDevice>()
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
                    .Cast<IGpuDevice>()
                    .Select(gpu => gpu.GpuRam);
                foreach (var ram in gpuRams) ramSum += ram;
                return ramSum;
            }
        }

        internal static void AddDevice(ComputeDevice dev)
        {
            _devices.Add(dev);
            if (dev.DeviceType != DeviceType.CPU) _gpus.Add(dev);
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

        private static int GetCountForType(DeviceType type)
        {
            return Devices.Count(device => device.DeviceType == type);
        }

        internal static void UncheckCpuIfGpu()
        {
            if (!HasGpu) return;
            bool isNotAMDZenCPU(ComputeDevice d) => d.BaseDevice is CPUDevice cpu && !cpu.CpuID.IsZen;
            var cpus = Devices.Where(isNotAMDZenCPU)
                              .ToArray();
            foreach (var dev in cpus) dev.Enabled = false;
        }

        public static int GetDeviceIndexFromUuid(string uuid)
        {
            var index = _gpus.FindIndex(gpu => gpu.Uuid == uuid);
            if (index >= 0) return index;
            return 0;
        }

        public static string GetDeviceUuidFromIndex(int index)
        {
            if (index < _gpus.Count) return _gpus[index].Uuid;
            return "";
        }
    }
}
