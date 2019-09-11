using NHM.Common.Enums;

namespace NHMCore.Stats
{
    public static class TimeFactor
    {
        public static double TimeUnit { get; private set; }

        private static TimeUnitType _unitType;

        public static TimeUnitType UnitType
        {
            get => _unitType;
            set
            {
                _unitType = value;
                switch (value)
                {
                    case TimeUnitType.Hour:
                        TimeUnit = 1.0 / 24.0;
                        break;
                    case TimeUnitType.Day:
                        TimeUnit = 1;
                        break;
                    case TimeUnitType.Week:
                        TimeUnit = 7;
                        break;
                    case TimeUnitType.Month:
                        TimeUnit = 30;
                        break;
                    case TimeUnitType.Year:
                        TimeUnit = 365;
                        break;
                }
            }
        }
    }
}
