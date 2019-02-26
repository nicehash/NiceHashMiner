using System;
using System.Collections.Generic;

namespace NiceHashMiner.Devices.Querying.Amd.OpenCL
{
    [Serializable]
    public class OpenCLPlatform
    {
        public List<OpenCLDevice> Devices { get; set; } = new List<OpenCLDevice>();
        public string PlatformName { get; set; } = "NONE";
        public int PlatformNum { get; set; } = 0;
    }
}
