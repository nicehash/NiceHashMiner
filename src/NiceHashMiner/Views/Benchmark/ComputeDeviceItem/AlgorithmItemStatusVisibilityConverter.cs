using NHMCore.Mining;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    class AlgorithmItemStatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlgorithmStatus state)
            {
                switch (state)
                {
                    case AlgorithmStatus.Benchmarking:
                    case AlgorithmStatus.Mining:
                        return Visibility.Visible;
                }
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
