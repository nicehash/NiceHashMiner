using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark
{
    class CpuGpuGroupVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                if (count == 0) return Visibility.Hidden;
                return Visibility.Visible;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
