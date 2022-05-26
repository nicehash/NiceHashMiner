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
        public List<OpenCLPlatform> Platforms { get; set; } = new List<OpenCLPlatform>();
        public string Status { get; set; }
        public List<AMDBusIDVersionResult> AMDBusIDVersionPairs { get; set; } = new List<AMDBusIDVersionResult>();
        public bool Equals(OpenCLDeviceDetectionResult other)
        {
            if (ErrorString != other.ErrorString) return false;
            if (Status != other.Status) return false;
            foreach (var thisPlatform in Platforms)
            {
                foreach (var otherPlatform in other.Platforms)
                {
                    if (!thisPlatform.Equals(otherPlatform)) return false;
                }
            }
            foreach (var thisAMDBusIDVersionPair in AMDBusIDVersionPairs)
            {
                foreach (var otherAMDBusIDVersionPair in other.AMDBusIDVersionPairs)
                {
                    if (thisAMDBusIDVersionPair != otherAMDBusIDVersionPair) return false;
                }
            }
            return true;
        }
    }
}
