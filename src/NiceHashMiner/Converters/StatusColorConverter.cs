using NHM.Common.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NiceHashMiner.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        private static SolidColorBrush DEFAULT = new SolidColorBrush(Color.FromArgb(50, 00, 150, 50));

        private static object ColorForDeviceState(DeviceState state) => state switch
        {
            DeviceState.Mining => Application.Current.FindResource("NastyGreenBrush"),
#if NHMWS4
            DeviceState.Benchmarking or DeviceState.Pending => Application.Current.FindResource("PrimaryColorBrush"),
#else
            DeviceState.Benchmarking or DeviceState.Pending or DeviceState.Testing => Application.Current.FindResource("PrimaryColorBrush"),
#endif
            DeviceState.Disabled => Application.Current.FindResource("Gray1ColorBrush"),
            DeviceState.Error => Application.Current.FindResource("RedDangerColorBrush"),
            DeviceState.Stopped => Application.Current.FindResource("TextColorBrush"),
            _ => DEFAULT,
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DeviceState state => ColorForDeviceState(state),
                _ => DEFAULT,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
