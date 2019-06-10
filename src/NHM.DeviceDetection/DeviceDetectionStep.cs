using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection
{
    public enum DeviceDetectionStep
    {
        CPU = 0,
        WMIWMIVideoControllers,
        NVIDIA_CUDA,
        AMD_OpenCL
    }
}
