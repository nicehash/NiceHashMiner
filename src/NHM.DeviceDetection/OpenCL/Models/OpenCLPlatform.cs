using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.OpenCL.Models
{
    [Serializable]
    internal class OpenCLPlatform : IEquatable<OpenCLPlatform>
    {
        public List<OpenCLDevice> Devices { get; set; } = new List<OpenCLDevice>();
        public string PlatformName { get; set; } = "NONE";
        public string PlatformVendor { get; set; } = "NONE";
        public int PlatformNum { get; set; } = -1;
        public bool Equals(OpenCLPlatform other)
        {
            if (PlatformName != other.PlatformName) return false;
            if (PlatformVendor != other.PlatformVendor) return false;
            if (PlatformNum != other.PlatformNum) return false;
            if (Devices.Count != other.Devices.Count) return false;
            foreach (var thisDevice in Devices)
            {
                foreach (var otherDevice in other.Devices)
                {
                    if (thisDevice != otherDevice) return false;
                }
            }
            return true;
        }
    }
}
