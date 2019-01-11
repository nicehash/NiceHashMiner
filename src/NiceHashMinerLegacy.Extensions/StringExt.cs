using System;
using System.Globalization;
using System.Linq;

namespace NiceHashMinerLegacy.Extensions
{
    public static class StringExt
    {
        public static bool TryGetHashrateAfter(this string s, string after, out double hashrate)
        {
            if (!s.Contains(after))
            {
                hashrate = default(double);
                return false;
            }

            var afterString = s.GetStringAfter(after).ToLower();
            var numString = new string(afterString
                .ToCharArray()
                .SkipWhile(c => !char.IsDigit(c))
                .TakeWhile(c => char.IsDigit(c) || c == '.')
                .ToArray());

            if (!double.TryParse(numString, NumberStyles.Float, CultureInfo.InvariantCulture, out var hash))
            {
                hashrate = default(double);
                return false;
            }

            if (afterString.Contains("kh"))
                hashrate = hash * 1000;
            else if (afterString.Contains("mh"))
                hashrate = hash * 1000000;
            else if (afterString.Contains("gh"))
                hashrate = hash * 1000000000;
            else
                hashrate = hash;

            return true;
        }

        public static string GetStringAfter(this string s, string after)
        {
            var index = s.IndexOf(after, StringComparison.Ordinal);
            return s.Substring(index);
        }
    }
}
