using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NiceHashMinerLegacy.Extensions
{
    public static class StringExt
    {
        public static double? GetHashrateAfter(this string s, string after)
        {
            var afterString = s.GetStringAfter(after).ToLower();
            var numString = new string(afterString
                .ToCharArray()
                .SkipWhile(c => !char.IsDigit(c))
                .TakeWhile(c => char.IsDigit(c) || c == '.')
                .ToArray());

            if (!double.TryParse(numString, out var hash))
            {
                return null;
            }

            if (afterString.Contains("kh"))
                return hash * 1000;
            if (afterString.Contains("mh"))
                return hash * 1000000;
            if (afterString.Contains("gh"))
                return hash * 1000000000;

            return hash;
        }

        public static string GetStringAfter(this string s, string after)
        {
            var index = s.IndexOf(after);
            return s.Substring(index);
        }
    }
}
