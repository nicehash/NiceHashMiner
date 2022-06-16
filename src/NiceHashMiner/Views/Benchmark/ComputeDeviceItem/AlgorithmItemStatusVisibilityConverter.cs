using NHMCore.Mining;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    class AlgorithmItemStatusVisibilityConverter : IValueConverter
    {
        private static bool IsStateVisible(AlgorithmStatus state)
        {
            return state switch
            {
                AlgorithmStatus.Benchmarking or AlgorithmStatus.Mining => true,
                _ => false,
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                AlgorithmStatus state when IsStateVisible(state) => Visibility.Visible,
                _ => Visibility.Hidden,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
