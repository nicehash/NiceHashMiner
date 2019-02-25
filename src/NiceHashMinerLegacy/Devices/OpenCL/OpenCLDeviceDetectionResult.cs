using System;
using System.Collections.Generic;

namespace NiceHashMiner.Devices.OpenCL
{
    [Serializable]
    public class OpenCLDeviceDetectionResult
    {
        public string ErrorString { get; set; }
        public List<OpenCLPlatform> Platforms { get; set; }
        public string Status { get; set; }
    }
}
