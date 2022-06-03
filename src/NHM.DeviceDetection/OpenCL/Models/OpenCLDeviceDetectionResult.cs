using NHM.DeviceDetection.Models.AMDBusIDVersionResult
;
using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.OpenCL.Models
{
    [Serializable]
    internal class OpenCLDeviceDetectionResult : IEquatable<OpenCLDeviceDetectionResult>
    {
        public string ErrorString { get; set; }
        public List<OpenCLPlatform> Platforms { get; set; }
        public string Status { get; set; }
        public List<AMDBusIDVersionResult> AMDBusIDVersionPairs { get; set; }
        public bool Equals(OpenCLDeviceDetectionResult other)
        {
            if (ErrorString != other.ErrorString) return false;
            if (Status != other.Status) return false;
            if(Platforms != null && other.Platforms != null)
            {
                if (Platforms.Count != other.Platforms.Count) return false;
                for (int i = 0; i < Platforms.Count; i++)
                {
                    if (!Platforms[i].Equals(other.Platforms[i])) return false;
                }
            }
            if (Platforms == null && other.Platforms != null ||
                Platforms != null && other.Platforms == null) return false;
            if(AMDBusIDVersionPairs != null && other.AMDBusIDVersionPairs != null)
            {
                if (AMDBusIDVersionPairs.Count != other.AMDBusIDVersionPairs.Count) return false;
                for (int i = 0; i < AMDBusIDVersionPairs.Count; i++)
                {
                    if (AMDBusIDVersionPairs[i] != other.AMDBusIDVersionPairs[i]) return false;
                }
            }
            if (AMDBusIDVersionPairs == null && other.AMDBusIDVersionPairs != null ||
                AMDBusIDVersionPairs != null && other.AMDBusIDVersionPairs == null) return false;
            return true;
        }
    }
}
