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
        private int[] IntDriverVersion = new int[4];

        private bool IsValidVersion()
        {
            string[] splitVerCurrent = DriverVersion.Split('.');
            if (splitVerCurrent.Length != 4) return false;
            var currentV = ToIntVersion();
            bool res = IsMoreThanMinimumVersion(currentV);
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
            return new Version(IntDriverVersion[0], IntDriverVersion[1], IntDriverVersion[2], IntDriverVersion[3]);
        }

        public Version ToIntVersion()
        {
            List<int> ver = new List<int>();
            var splitVerCurrent = DriverVersion.Split('.');
            if (splitVerCurrent.Length != 4)
            {
                IntDriverVersion.SetValue(-1, new int[4] { 0, 1, 2, 3 });
            }

            for (int i = 0; i < 4; i++)
            {
                int num;
                if (Int32.TryParse(splitVerCurrent[i], out num))
                {
                    IntDriverVersion[i] = num;
                }
                else
                {
                    IntDriverVersion[i] = -1;
                }
            }
            if (IntDriverVersion.Contains(-1))
            {
                CorrectFormat = false;
                return new Version(-1,-1,-1,-1);
            }
            else
            {
                CorrectFormat = true;
                return new Version(IntDriverVersion[0], IntDriverVersion[1], IntDriverVersion[2], IntDriverVersion[3]);
            }
        }

    }
}
