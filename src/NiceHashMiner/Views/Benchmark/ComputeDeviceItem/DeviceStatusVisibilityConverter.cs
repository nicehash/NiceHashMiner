using NHM.Common.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    class DeviceStatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceState state)
            {
#if NHMWS4
                if (state == DeviceState.Benchmarking || state == DeviceState.Mining || state == DeviceState.Testing)
#else
                if (state == DeviceState.Benchmarking || state == DeviceState.Mining)
#endif
                {
                    return Visibility.Hidden;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
