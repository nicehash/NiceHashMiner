using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.OpenCL.Models
{
    [Serializable]
    internal class OpenCLDeviceDetectionResult
    {
        public string ErrorString { get; set; }
        public List<OpenCLPlatform> Platforms { get; set; }
        public string Status { get; set; }
    }
}
