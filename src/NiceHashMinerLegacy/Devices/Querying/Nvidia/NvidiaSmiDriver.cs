using System;

namespace NiceHashMiner.Devices.Querying.Nvidia
{
    // format 372.54;
    internal struct NvidiaSmiDriver : IComparable<NvidiaSmiDriver>
    {
        public int LeftPart { get; }

        private readonly int _rightPart;
        public int RightPart
        {
            get
            {
                if (_rightPart >= 10)
                {
                    return _rightPart;
                }

                return _rightPart * 10;
            }
        }

        public NvidiaSmiDriver(int left, int right)
        {
            LeftPart = left;
            _rightPart = right;
        }

        public override string ToString()
        {
            return $"{LeftPart}.{RightPart}";
        }

        #region IComparable implementation

        public int CompareTo(NvidiaSmiDriver other)
        {
            var leftPartComparison = LeftPart.CompareTo(other.LeftPart);
            if (leftPartComparison != 0) return leftPartComparison;
            return RightPart.CompareTo(other.RightPart);
        }

        public static bool operator <(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion
    }
}
