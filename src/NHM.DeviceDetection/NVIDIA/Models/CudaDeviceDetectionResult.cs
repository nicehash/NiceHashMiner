using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.NVIDIA.Models
{
    [Serializable]
    internal class CudaDeviceDetectionResult : IEquatable<CudaDeviceDetectionResult>
    {
        public List<CudaDevice> CudaDevices { get; set; } = new List<CudaDevice>();
        public string DriverVersion { get; set; }
        public string ErrorString { get; set; }
        public int NvmlLoaded { get; set; }
        public int NvmlInitialized { get; set; }

        public bool Equals(CudaDeviceDetectionResult other)
        {
            if (DriverVersion != other.DriverVersion) return false;
            if (NvmlLoaded != other.NvmlLoaded) return false;
            if (ErrorString != other.ErrorString) return false;
            if (NvmlInitialized != other.NvmlInitialized) return false;
            foreach(var thisCudaDevice in CudaDevices)
            {
                foreach(var otherCudaDevice in other.CudaDevices)
                {
                    if(otherCudaDevice != thisCudaDevice) return false;
                }
            }
            return true;
        }
    }
}
