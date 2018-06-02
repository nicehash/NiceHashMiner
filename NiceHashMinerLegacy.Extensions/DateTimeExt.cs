using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Extensions
{
    public static class DateTimeExt
    {
        private static readonly DateTime UnixStart = new DateTime(1970, 1, 1);

        public static ulong GetUnixTime(this DateTime time)
        {
            return (ulong) (time.ToUniversalTime() - UnixStart).TotalMilliseconds;
        }
    }
}
