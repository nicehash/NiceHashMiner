using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices
{
    [Serializable]
    class CudaDeviceDetectionResult
    {
        public List<CudaDevice> CudaDevices;
        public string DriverVersion;
        public string ErrorString;
    }
}
