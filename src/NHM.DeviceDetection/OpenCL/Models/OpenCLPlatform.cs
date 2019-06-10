using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.OpenCL.Models
{
    [Serializable]
    internal class OpenCLPlatform
    {
        public List<OpenCLDevice> Devices { get; set; } = new List<OpenCLDevice>();
        public string PlatformName { get; set; } = "NONE";
        public string PlatformVendor { get; set; } = "NONE";
        public int PlatformNum { get; set; } = -1;
    }
}
