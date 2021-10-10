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
        private bool CorrectFormat;
        public bool IsValid;
        public bool IsCorrectVersion;
        public AmdDriver(string DriveStoreVersion)
        {
            _driverVersion = DriveStoreVersion;
            IsCorrectVersion = IsValidVersion();
            IsValid = CorrectFormat;
        }

        private string _driverVersion { get; set; }
        public string DriverVersion
        {
            get
            {
                return _driverVersion;
            }
        }
        public Version VerDriverVersion;
        private bool IsValidVersion()
        {
            string[] splitVerCurrent = DriverVersion.Split('.');
            if (splitVerCurrent.Length != 4) return false;
            VerDriverVersion = ToVersion();
            bool res = IsMoreThanMinimumVersion(VerDriverVersion);
            return res;
        }

        private bool IsMoreThanMinimumVersion(Version thisV)
        {
            (int current, int min) delta = (-1,-1);
            if (thisV.Major != MinimumVersion.DriveStoreFormat.Major) delta = (thisV.Major, MinimumVersion.DriveStoreFormat.Major);
            else if (thisV.Minor != MinimumVersion.DriveStoreFormat.Minor) delta = (thisV.Minor, MinimumVersion.DriveStoreFormat.Minor);
            else if (thisV.Build != MinimumVersion.DriveStoreFormat.Build) delta = (thisV.Build, MinimumVersion.DriveStoreFormat.Build);
            else if (thisV.Revision != MinimumVersion.DriveStoreFormat.Revision) delta = (thisV.Revision, MinimumVersion.DriveStoreFormat.Revision);

            if(delta != (-1, -1))
            {
                if (delta.current >= delta.min) return true;
                else return false;
            }

            return true;
        }


        public Version ToVersion()
        {
            int major = -1;
            int minor = -1;
            int build = -1;
            int rev = -1;
            List<int> ver = new List<int>();
            var splitVerCurrent = DriverVersion.Split('.');
            if (splitVerCurrent.Length != 4)
            {
                return new Version(major, minor, build, rev);
            }

            Int32.TryParse(splitVerCurrent[0], out major);
            Int32.TryParse(splitVerCurrent[1], out minor);
            Int32.TryParse(splitVerCurrent[2], out build);
            Int32.TryParse(splitVerCurrent[3], out rev);


            if (major == -1 || minor == -1 || build == -1 || rev == -1)
            {
                CorrectFormat = false;
                return new Version(-1,-1,-1,-1);
            }
            else
            {
                CorrectFormat = true;
                return new Version(major, minor, build, rev);
            }
        }

    }
}
