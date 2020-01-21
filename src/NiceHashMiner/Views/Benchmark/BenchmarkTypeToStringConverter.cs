using NHM.Common.Enums;
using NHMCore;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark
{
    class BenchmarkTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BenchmarkPerformanceType benchmarkPerformanceType)
            {
                if (BenchmarkPerformanceType.Precise == benchmarkPerformanceType) return Translations.Tr("Precise (will take longer)");
                if (BenchmarkPerformanceType.Quick == benchmarkPerformanceType) return Translations.Tr("Quick (can be inaccurate)");
            }
            return Translations.Tr("Standard");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
