using NHM.DeviceDetection.NVIDIA;
using System;

namespace NHM.DeviceDetection.WMI
{
    internal static class WmiNvidiaDriverParser
    {
        public static NvidiaSmiDriver ParseNvSmiDriver(string windowsDriverVersion)
        {
            var winVerArray = windowsDriverVersion.Split('.');
            //we must parse windows driver format (ie. 25.21.14.1634) into nvidia driver format (ie. 416.34)
            //nvidia format driver is inside last two elements of splited windows driver string (ie. 14 + 1634)
            if (winVerArray.Length >= 2)
            {
                var firstPartOfVersion = winVerArray[winVerArray.Length - 2];
                var secondPartOfVersion = winVerArray[winVerArray.Length - 1];
                var shortVerArray = firstPartOfVersion + secondPartOfVersion;
                var driverFull = shortVerArray.Remove(0, 1).Insert(3, ".").Split('.'); // we transform that string into "nvidia" version (ie. 416.83)
                var driver = new NvidiaSmiDriver(Convert.ToInt32(driverFull[0]), Convert.ToInt32(driverFull[1])); //we create driver object from string version

                return driver;
            }
            return new NvidiaSmiDriver(-1, -1);
        }
    }
}
