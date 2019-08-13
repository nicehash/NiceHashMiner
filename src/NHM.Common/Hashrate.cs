using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHM.Common.Enums;

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

        public override string ToString()
        {
            return ToString(" ");
        }

        public string ToString(string separator)
        {
            var speed = "";

            if (Value < 1000)
                speed = Value.ToString("F3", CultureInfo.InvariantCulture) + separator;
            else if (Value < 100000)
                speed = (Value * 0.001).ToString("F3", CultureInfo.InvariantCulture) + separator + "k";
            else if (Value < 100000000)
                speed = (Value * 0.000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "M";
            else
                speed = (Value * 0.000000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "G";

            return speed + GetUnitForAlgorithmType(Algo);
        }

        // Quickfix copy here since Helpers not accessible in Common
        private static string GetUnitForAlgorithmType(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                //case AlgorithmType.Equihash:
                case AlgorithmType.ZHash:
                case AlgorithmType.Beam:
                    return "Sol/s";
                case AlgorithmType.GrinCuckaroo29:
                case AlgorithmType.GrinCuckatoo31:
                case AlgorithmType.CuckooCycle:
                    return "G/s";
                default:
                    return "H/s";
            }
        }
    }
}
