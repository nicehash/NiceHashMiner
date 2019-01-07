using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Stats
{
    public static class TimeFactor
    {
        public static double TimeUnit { get; private set; }

        public static void UpdateTimeUnit(TimeUnitType unit)
        {
            switch (unit)
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
