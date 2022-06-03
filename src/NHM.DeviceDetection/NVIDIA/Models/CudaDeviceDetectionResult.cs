using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.NVIDIA.Models
{
    [Serializable]
    internal class CudaDeviceDetectionResult : IEquatable<CudaDeviceDetectionResult>
    {
        public List<CudaDevice> CudaDevices { get; set; }
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
            if(CudaDevices != null && other.CudaDevices != null)
            {
                if (CudaDevices.Count != other.CudaDevices.Count) return false;
                for (int i = 0; i < CudaDevices.Count; i++)
                {
                    if (CudaDevices[i] != other.CudaDevices[i]) return false;
                }
            }
            if (CudaDevices == null && other.CudaDevices != null ||
                CudaDevices != null && other.CudaDevices == null) return false;
            return true;
        }
    }
}
