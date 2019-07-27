using System;
using System.Globalization;
using System.Windows.Data;

namespace NHM.Wpf.ViewModels.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d;

            // Allow some other convertible num types
            switch (value)
            {
                case double d1:
                    d = d1;
                    break;
                case float f:
                    d = f;
                    break;
                case int i:
                    d = i;
                    break;
                default:
                    throw new ArgumentException();
            }

            return d < 0 ? Translations.Tr("N/A") : d.ToString("F0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
