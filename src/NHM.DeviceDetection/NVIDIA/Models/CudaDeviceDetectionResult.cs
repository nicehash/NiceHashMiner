using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.NVIDIA.Models
{
    [Serializable]
    internal class CudaDeviceDetectionResult
    {
        public List<CudaDevice> CudaDevices { get; set; }
        public string DriverVersion { get; set; }
        public string ErrorString { get; set; }
        public int NvmlLoaded { get; set; }
        public int NvmlInitialized { get; set; }
    }
}
