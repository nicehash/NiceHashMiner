using System;

namespace NHM.DeviceDetection.NVIDIA
{
    // format 372.54;
    internal class NvidiaSmiDriver
    {
        public static readonly Version MinimumVersion = new Version(461, 33);

        public bool IsCorrectVersion { get; }
        public bool IsValid { get; }
        public Version Version { get; } = null;

        public static NvidiaSmiDriver ToNvidiaSmiDriver(string strVersion)
        {
            if (Version.TryParse(strVersion, out var v)) return new NvidiaSmiDriver(v.Major, v.Minor);
            return new NvidiaSmiDriver(-1, -1);
        }

        public NvidiaSmiDriver(int left, int right)
        {
            IsValid = left > -1 && right > -1;
            if (IsValid) Version = new Version(left, right >= 10 ? right : (right * 10));
            IsCorrectVersion = IsValid && Version >= MinimumVersion;
        }

        public override string ToString()
        {
            return IsValid ? $"{Version.Major}.{Version.Minor}" : "N/A";
        }
    }
}
