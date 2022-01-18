using NHM.DeviceDetection.NVIDIA;
using System;
using System.Linq;

namespace NHM.DeviceDetection.WMI
{
    internal static class WmiNvidiaDriverParser
    {
        public static NvidiaSmiDriver ParseNvSmiDriver(string windowsDriverVersion)
        {
            // We must parse windows driver format (ie. 25.21.14.1634) into nvidia driver format (ie. 416.34)
            // NVIDIA format driver is inside last two elements of splited windows driver string (ie. 14 + 1634) we transform that string into NVIDIA version (ie. 416.34)
            var mergedReversedVersionsString = string.Join("", windowsDriverVersion.Replace(".", "").Reverse());
            if (mergedReversedVersionsString.Length >= 5)
            {
                var left = string.Join("", mergedReversedVersionsString.Skip(2).Take(3).Reverse());
                var right = string.Join("", mergedReversedVersionsString.Take(2).Reverse());
                var driver = new NvidiaSmiDriver(Convert.ToInt32(left), Convert.ToInt32(right));

                return driver;
            }
            return new NvidiaSmiDriver(-1, -1);
        }
    }
}
