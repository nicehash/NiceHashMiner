using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.NVIDIA.Models
{
    [Serializable]
    internal class CudaDeviceDetectionResult
    {
        public List<CudaDevice> CudaDevices { get; set; }
        public string DriverVersion { get; set; }
        public string ErrorString { get; set; }
        public bool NvmlLoaded { get; set; }
        public bool NvmlLoadedFallback { get; set; } = false;
    }
}
