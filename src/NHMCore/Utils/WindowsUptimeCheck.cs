using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    internal static class WindowsUptimeCheck
    {
        public static async Task DelayUptime()
        {
            var waitSecondsBeforeStart = WaitSecondsBeforeStart();
            if (waitSecondsBeforeStart > 0)
            {
                await Task.Delay((int)waitSecondsBeforeStart * 1000);
            }
        }

        public static double WaitSecondsBeforeStart()
        {
            var waitSeconds = 10 - GetUpTime().TotalSeconds;
            return waitSeconds;
        }

        public static TimeSpan GetUpTime()
        {
            return TimeSpan.FromMilliseconds(GetTickCount64());
        }

        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
    }
}
