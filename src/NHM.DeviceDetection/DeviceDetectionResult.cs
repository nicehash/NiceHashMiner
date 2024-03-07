﻿using NHM.Common.Device;
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
        public IReadOnlyList<CUDADevice> CUDADevices { get; internal set; } = new List<CUDADevice>();
        public bool HasCUDADevices => CUDADevices != null && CUDADevices.Count > 0;
        public Version NvidiaDriver { get; internal set; }
        public bool NVIDIADriverObsolete { get; internal set; }


        public bool IsNvidiaNVMLLoadedError { get; internal set; }
        public bool IsNvidiaNVMLInitializedError { get; internal set; }
        public IReadOnlyList<CUDADevice> UnsupportedCUDADevices { get; internal set; }

        // AMD
        public IReadOnlyList<AMDDevice> AMDDevices { get; internal set; } = new List<AMDDevice>();
        public bool HasAMDDevices => AMDDevices != null && AMDDevices.Count > 0;
        public Version AmdDriver { get; internal set; }
        public bool AMDDriverObsolete { get; internal set; }

        // INTEL
        public IReadOnlyList<IntelDevice> IntelDevices { get; internal set; } = new List<IntelDevice>();
        public bool HasIntelDevices => IntelDevices != null && IntelDevices.Count > 0;
        public Version IntelDriver { get; internal set; }

        // FAKE
        public IReadOnlyList<FakeDevice> FAKEDevices { get; internal set; }
    }
}
