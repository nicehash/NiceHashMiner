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
            switch (state)
            {
                case AlgorithmStatus.Benchmarking:
                case AlgorithmStatus.Mining:
                    return true;
                default:
                    return false;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case AlgorithmStatus state when IsStateVisible(state):
                    return Visibility.Visible;
                default:
                    return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
