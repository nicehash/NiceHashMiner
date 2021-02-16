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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return Translations.Tr(value);
            if (value is DeviceState state)
            {
                switch (state)
                {
                    case DeviceState.Mining:
                        return Application.Current.FindResource("NastyGreenBrush");
                    case DeviceState.Benchmarking:
                    case DeviceState.Pending:
                        return Application.Current.FindResource("PrimaryColorBrush");
                    case DeviceState.Disabled:
                        return Application.Current.FindResource("Gray1ColorBrush");
                    case DeviceState.Error:
                        return Application.Current.FindResource("RedDangerColorBrush");
                    case DeviceState.Stopped:
                        return Application.Current.FindResource("TextColorBrush");
                        //default:

                }
            }
            return DEFAULT;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
