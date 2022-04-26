using NHM.Common.Enums;
using System.Globalization;

namespace NHM.Common
{
    public struct Hashrate
    {
        public double Value { get; set; }

        public AlgorithmType Algo { get; set; }

        public Hashrate(double value, AlgorithmType algo)
        {
            Value = value;
            Algo = algo;
        }

        public static implicit operator double(Hashrate h)
        {
            return h.Value;
        }

        public static implicit operator Hashrate(double d)
        {
            return new Hashrate(d, AlgorithmType.NONE);
        }

        public static implicit operator Hashrate((AlgorithmType, double) tuple)
        {
            return new Hashrate(tuple.Item2, tuple.Item1);
        }

        public static Hashrate operator +(Hashrate left, Hashrate right)
        {
            return new Hashrate(left.Value + right.Value, left.Algo);
        }

        public override string ToString()
        {
            return ToString(Algo, Value, " ");
        }

        public static string ToString(AlgorithmType algorithmType, double rawSpeed, string separator = " ")
        {
            var (scaledSpeed, unit) = rawSpeed switch
            {
                < 1000 => (rawSpeed, separator),
                < 100000 => (rawSpeed * 0.001, $"{separator}k"),
                < 100000000 => (rawSpeed * 0.000001, $"{separator}M"),
                _ => (rawSpeed * 0.000000001, $"{separator}G"),
            };
            var speed = scaledSpeed.ToString("F3", CultureInfo.InvariantCulture) + unit + algorithmType.GetUnitPerSecond();
            return speed;
        }
    }
}
