using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WildRig
{
    internal class JsonApiResponse
    {
        public Hashrate hashrate { get; set; }
    }

    internal class Hashrate
    {
        public List<List<int>> threads { get; set; }
    }

    internal static class BenchmarkHelpers
    {
        public static Tuple<double, bool> TryGetHashrateAfter(this string s, string after)
        {
            if (!s.Contains(after))
            {
                return Tuple.Create(0d, false); ;
            }

            var afterString = s.GetStringAfter(after).ToLower();
            var na = afterString.Substring(0, 4);

            if (na.Contains("n/a")) return Tuple.Create(0d, false);

            var numString = new string(afterString
                .ToCharArray()
                .SkipWhile(c => !char.IsDigit(c))
                .TakeWhile(c => char.IsDigit(c) || c == '.')
                .ToArray());

            if (!double.TryParse(numString, NumberStyles.Float, CultureInfo.InvariantCulture, out var hash))
            {
                return Tuple.Create(0d, false); ;
            }

            var afterNumString = afterString.GetStringAfter(numString);
            var splitedString = afterNumString.Split(' ');
            var postfixString = splitedString.LastOrDefault();

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
