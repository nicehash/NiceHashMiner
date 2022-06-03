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
            if (Devices.Count != other.Devices.Count) return false;
            if(Devices != null && other.Devices != null)
            {
                for (int i = 0; i < Devices.Count; i++)
                {
                    if (Devices[i] != other.Devices[i]) return false;
                }
            }
            if (Devices == null && other.Devices != null ||
                Devices != null && other.Devices == null) return false;
            return true;
        }
    }
}
