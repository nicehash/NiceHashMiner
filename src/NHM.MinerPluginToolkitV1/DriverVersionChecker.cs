using NHM.Common.Device;
using NHM.Common.Enums;
using System;

namespace NHM.MinerPluginToolkitV1
{
    public static class DriverVersionChecker
    {
        public static (DriverVersionCheckType ret, Version minRequired) CompareCUDADriverVersions(BaseDevice dev, Version installedVersion, Version minVersion)
        {
            if (dev is CUDADevice) {
                if (installedVersion < minVersion) return (DriverVersionCheckType.DriverVersionObsolete, minVersion);
                return (DriverVersionCheckType.DriverVersionOK, installedVersion);
            }
            return (DriverVersionCheckType.DriverCheckNotImplementedForThisDeviceType, new Version(0, 0));
        }

        public static (DriverVersionCheckType ret, Version minRequired) CompareAMDDriverVersions(BaseDevice dev, Version minVersion)
        {
            if (dev is AMDDevice amd)
            {
                if (amd.DEVICE_AMD_DRIVER < minVersion) return (DriverVersionCheckType.DriverVersionObsolete, minVersion);
                return (DriverVersionCheckType.DriverVersionOK, amd.DEVICE_AMD_DRIVER);
            }
            return (DriverVersionCheckType.DriverCheckNotImplementedForThisDeviceType, new Version(0, 0));
        }
    }
}
