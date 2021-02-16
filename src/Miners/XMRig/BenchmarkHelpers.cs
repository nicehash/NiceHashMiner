using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace XMRig
{
    public static class BenchmarkHelpers
    {
        public static Tuple<double, bool> TryGetHashrateAfter(this string s, string contains)
        {
            if (!s.Contains(contains))
            {
                return Tuple.Create(0d, false); ;
            }
            s = s.Substring(s.IndexOf(contains));
            var splittedString = s.Split(' ');

            var afterString = splittedString[2].ToLower();
            var numString = new string(afterString
                .ToCharArray()
                .SkipWhile(c => !char.IsDigit(c))
                .TakeWhile(c => char.IsDigit(c) || c == '.')
                .ToArray());

            if (!double.TryParse(numString, NumberStyles.Float, CultureInfo.InvariantCulture, out var hash))
            {
                return Tuple.Create(0d, false);
            }

            var postfixString = splittedString[5];
            for (var i = 0; i < postfixString.Length - 1; ++i)
            {
                var c = postfixString[i];
                if (!Char.IsLetter(c)) continue;
                var c2 = postfixString[i + 1];

                foreach (var kvp in _postfixes)
                {
                    var postfix = Char.ToLower(kvp.Key);
                    var mult = kvp.Value;
                    if (postfix == c && 'h' == c2)
                    {
                        var hashrate = hash * mult;
                        return Tuple.Create(hashrate, true);
                    }
                }
            }
            return Tuple.Create(hash, true);
        }

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
    }
}
