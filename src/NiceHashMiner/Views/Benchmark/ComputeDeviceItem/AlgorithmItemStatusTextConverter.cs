using NHMCore.Mining;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NHMCore;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    class AlgorithmItemStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Translations.Tr(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
