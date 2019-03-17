using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NiceHashMinerLegacy.Extensions
{
    public static class StringExt
    {
        private static int pow10(int power) => (int)Math.Pow(10, power);
        private static readonly Dictionary<char, int> _postfixes = new Dictionary<char, int>
        {
            {'k', pow10(3)},
            {'M', pow10(6)},
            {'G', pow10(9)},
            {'T', pow10(12)},
            {'P', pow10(15)},
            {'E', pow10(15)},
            {'Z', pow10(21)},
            {'Y', pow10(24)},
        };

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

            var afterNumString = afterString.GetStringAfter(numString);
            for (var i = 0; i < afterNumString.Length - 1; ++i)
            {
                var c = afterNumString[i];
                if (!Char.IsLetter(c)) continue;
                var c2 = afterNumString[i + 1];

                foreach (var kvp in _postfixes)
                {
                    var postfix = Char.ToLower(kvp.Key);
                    var mult = kvp.Value;
                    if (postfix == c && 'h' == c2)
                    {
                        hashrate = hash * mult;
                        return true;
                    }
                }
            }
            hashrate = hash;
            return true;
        }

        public static string GetStringAfter(this string s, string after)
        {
            var index = s.IndexOf(after, StringComparison.Ordinal);
            return s.Substring(index + after.Length);
        }
    }
}
