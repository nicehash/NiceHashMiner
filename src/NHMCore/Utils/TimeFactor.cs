using NHM.Common.Enums;
using System;

namespace NHMCore.Utils
{
    public static class TimeFactor
    {
        public static double TimeUnit { get; private set; }

        private static TimeUnitType _unitType = TimeUnitType.Day;

        private static double GetTimeUnit(TimeUnitType type) => type switch
        {
            TimeUnitType.Hour => 1.0 / 24.0,
            TimeUnitType.Day => 1,
            TimeUnitType.Week => 7,
            TimeUnitType.Month => 30,
            TimeUnitType.Year => 365,
            _ => 1, // Day
        };

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

        public static double ConvertFromDay(double value) => value * TimeUnit;
    }
}
