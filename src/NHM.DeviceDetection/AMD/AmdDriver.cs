using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.AMD
{
    internal class AmdDriver
    {
        //Table of versions: https://gpuopen.com/version-table/
        public static readonly (Version DriveStoreFormat, Version AdrenalinFormat) MinimumVersion = (new Version(27,20,21003,8013), new Version(21,5,2));
        
        public bool IsValid { get; private set; }
        public bool IsCorrectVersion { get; private set; } // TODO rename

        public Version VerDriverVersion { get; private set; }

        public AmdDriver(string windowsDriverStoreVersion)
        {
            (IsValid, VerDriverVersion) = ToVersion(windowsDriverStoreVersion);
            IsCorrectVersion = IsValid && VerDriverVersion >= MinimumVersion.DriveStoreFormat;
        }

        private static bool IsValidWindowsVersion(string windowsDriverStoreVersion)
        {
            string[] splitVerCurrent = windowsDriverStoreVersion.Split('.');
            return splitVerCurrent.Length == 4 && splitVerCurrent.All(s => int.TryParse(s, out var _));
        }

        private static (bool isValid, Version version) ToVersion(string windowsDriverStoreVersion)
        {
            if (!IsValidWindowsVersion(windowsDriverStoreVersion)) return (false, null);
            try
            {
                return (true, new Version(windowsDriverStoreVersion));
            }
            catch (Exception)
            {
                return (false, null);
            }
            
        }

    }
}
