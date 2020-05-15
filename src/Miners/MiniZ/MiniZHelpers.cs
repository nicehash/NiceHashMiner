using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MiniZ
{
    [Serializable]
    internal class Result
    {
        public uint gpuid { get; set; }
        public uint cudaid { get; set; }
        public string busid { get; set; }
        public uint gpu_status { get; set; }
        public int temperature { get; set; }
        public uint gpu_power_usage { get; set; }
        public uint speed_sps { get; set; }
    }

    [Serializable]
    internal class JsonApiResponse
    {
        public object error { get; set; }
        public List<Result> result { get; set; }
    }
    internal static class BenchmarkHelpers
    {
        public static Tuple<double, bool> TryGetHashrateAfter(this string s, string after)
        {
            if (!s.Contains(after))
            {
                return Tuple.Create(0d, false); ;
            }

            var hashrateString = s.Substring(s.IndexOf(after) + 3);
            var hashrateArray = hashrateString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            var hashrateResult = hashrateArray.Split('(').FirstOrDefault();

            if (!double.TryParse(hashrateResult, NumberStyles.Float, CultureInfo.InvariantCulture, out var hash))
            {
                return Tuple.Create(0d, false); ;
            }

            var postfixString = hashrateArray.GetStringAfter(")");

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
