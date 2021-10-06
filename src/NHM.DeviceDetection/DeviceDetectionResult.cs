using NHM.Common.Device;
using NHM.DeviceDetection.WMI;
using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection
{
    public class DeviceDetectionResult
    {
        // WMI VideoControllers
        public IReadOnlyList<VideoControllerData> AvailableVideoControllers { get; internal set; }
        public Version NvidiaDriverWMI { get; internal set; }

        // CPU
        public CPUDevice CPU { get; internal set; }

        // NVIDIA
        public IReadOnlyList<CUDADevice> CUDADevices { get; internal set; }
        public bool IsDCHDriver { get; internal set; }
        public Version NvidiaDriver { get; internal set; }
        public bool IsNvidiaNVMLLoadedError { get; internal set; }
        public bool IsNvidiaNVMLInitializedError { get; internal set; }
        public IReadOnlyList<CUDADevice> UnsupportedCUDADevices { get; internal set; }

        // AMD
        public IReadOnlyList<AMDDevice> AMDDevices { get; internal set; }
        public bool IsOpenClFallback { get; internal set; }

        // FAKE
        public IReadOnlyList<FakeDevice> FAKEDevices { get; internal set; }
    }
}
