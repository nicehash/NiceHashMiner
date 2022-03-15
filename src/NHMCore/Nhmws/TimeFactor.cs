using NHM.Common.Enums;
using System;

namespace NHMCore.Nhmws
{
    public static class TimeFactor
    {
        public static double TimeUnit { get; private set; }

        private static TimeUnitType _unitType = TimeUnitType.Day;

        private static double GetTimeUnit(TimeUnitType type)
        {
            switch (type)
            {
                case TimeUnitType.Hour: return 1.0 / 24.0;
                case TimeUnitType.Day: return 1;
                case TimeUnitType.Week: return 7;
                case TimeUnitType.Month: return 30;
                case TimeUnitType.Year: return 365;
                default: return 1; // Day
            }
        }

        public static TimeUnitType UnitType
        {
            get => _unitType;
            set
            {
                _unitType = value;
                TimeUnit = GetTimeUnit(value);
                OnUnitTypeChanged?.Invoke(null, value);
            }
        }

        public static event EventHandler<TimeUnitType> OnUnitTypeChanged;

        public static double ConvertFromDay(double value)
        {
            return value * TimeUnit;
        }
    }
}
