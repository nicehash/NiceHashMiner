using NHM.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    class AlgorithmItemSpeedsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IReadOnlyList<Hashrate> speeds)
            {
                var hasNonZeroSpeeds = speeds.Sum(s => s.Value) > 0;
                if (hasNonZeroSpeeds)
                {
                    return string.Join(" + ", speeds.Select(s => s.ToString()));
                }
            }
            return "---";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
