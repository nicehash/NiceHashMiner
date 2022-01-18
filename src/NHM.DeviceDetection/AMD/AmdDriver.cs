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
            IsValid = Version.TryParse(windowsDriverStoreVersion, out var version);
            VerDriverVersion = IsValid ? version : null;
            IsCorrectVersion = IsValid && VerDriverVersion >= MinimumVersion.DriveStoreFormat;
        }
    }
}
