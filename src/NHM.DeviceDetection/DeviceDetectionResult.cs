using NHM.Common.Device;
using NHM.DeviceDetection.WMI;
using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection
{
    public class DeviceDetectionResult : IEquatable<DeviceDetectionResult>
    {
        // WMI VideoControllers
        public IReadOnlyList<VideoControllerData> AvailableVideoControllers { get; internal set; } = new List<VideoControllerData>();
        public Version NvidiaDriverWMI { get; internal set; }

        // CPU
        public CPUDevice CPU { get; internal set; }

        // NVIDIA
        public IReadOnlyList<CUDADevice> CUDADevices { get; internal set; } = new List<CUDADevice>();
        public bool HasCUDADevices => CUDADevices != null && CUDADevices.Count > 0;
        public bool IsDCHDriver { get; internal set; }
        public Version NvidiaDriver { get; internal set; }
        public bool NVIDIADriverObsolete { get; internal set; }


        public bool IsNvidiaNVMLLoadedError { get; internal set; }
        public bool IsNvidiaNVMLInitializedError { get; internal set; }
        public IReadOnlyList<CUDADevice> UnsupportedCUDADevices { get; internal set; } = new List<CUDADevice>();

        // AMD
        public IReadOnlyList<AMDDevice> AMDDevices { get; internal set; } = new List<AMDDevice>();
        public bool HasAMDDevices => AMDDevices != null && AMDDevices.Count > 0;
        public bool IsOpenClFallback { get; internal set; }
        public Version AmdDriver { get; internal set; }
        public bool AMDDriverObsolete { get; internal set; }



        // FAKE
        public IReadOnlyList<FakeDevice> FAKEDevices { get; internal set; }
        public bool Equals(DeviceDetectionResult other)
        {
            foreach (var vcd in AvailableVideoControllers)
            {
                foreach (var otherVcd in other.AvailableVideoControllers)
                {
                    if (vcd != otherVcd) return false;
                }
            }
            if (NvidiaDriverWMI != other.NvidiaDriverWMI) return false;
            if (!CPU.Equals(other.CPU)) return false;

            foreach (var cudaDev in CUDADevices)
            {
                foreach (var cudaDev2 in CUDADevices)
                {
                    if (!cudaDev.Equals(cudaDev2)) return false;
                }
            }
            if (IsDCHDriver != other.IsDCHDriver) return false;
            if (NvidiaDriver != other.NvidiaDriver) return false;
            if (NVIDIADriverObsolete != other.NVIDIADriverObsolete) return false;
            if (IsNvidiaNVMLLoadedError != other.IsNvidiaNVMLLoadedError) return false;
            if (IsNvidiaNVMLInitializedError != other.IsNvidiaNVMLInitializedError) return false;
            foreach (var cudaDev in UnsupportedCUDADevices)
            {
                foreach (var cudaDev2 in other.UnsupportedCUDADevices)
                {
                    if (!cudaDev.Equals(cudaDev2)) return false;
                }
            }
            foreach (var amdDev in AMDDevices)
            {
                foreach (var amdDev2 in AMDDevices)
                {
                    if (!amdDev.Equals(amdDev2)) return false;
                }
            }
            if (IsOpenClFallback != other.IsOpenClFallback) return false;
            if (AmdDriver != other.AmdDriver) return false;
            if (AMDDriverObsolete != other.AMDDriverObsolete) return false;
            foreach(var fake in FAKEDevices)
            {
                foreach(var fake2 in other.FAKEDevices)
                {
                    if(fake2.Equals(fake2)) return false;
                }
            }
            return true;
        }
    }
}
