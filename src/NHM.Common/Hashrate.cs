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
            var speed = "";

            if (rawSpeed < 1000)
                speed = rawSpeed.ToString("F3", CultureInfo.InvariantCulture) + separator;
            else if (rawSpeed < 100000)
                speed = (rawSpeed * 0.001).ToString("F3", CultureInfo.InvariantCulture) + separator + "k";
            else if (rawSpeed < 100000000)
                speed = (rawSpeed * 0.000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "M";
            else
                speed = (rawSpeed * 0.000000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "G";

            var ret = speed + algorithmType.GetUnitPerSecond();
            return ret;
        }
    }
}
