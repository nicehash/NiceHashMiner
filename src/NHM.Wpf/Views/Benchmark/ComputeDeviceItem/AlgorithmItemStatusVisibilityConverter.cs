using NHMCore.Mining;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NHM.Wpf.Views.Benchmark.ComputeDeviceItem
{
    class AlgorithmItemStatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bVal && !bVal)
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
