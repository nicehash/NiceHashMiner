﻿namespace NHM.DeviceDetection
{
    public enum DeviceDetectionStep
    {
        CPU = 0,
        WMIVideoControllers,
        NVIDIA_CUDA,
        AMD_OpenCL,
        INTEL_GPU,
        FAKE
    }
}
