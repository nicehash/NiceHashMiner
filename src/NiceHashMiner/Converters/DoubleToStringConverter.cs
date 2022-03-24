using NHMCore;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Converters
{
    /// <summary>
    /// Converts various number types to a string with no decimals, or "N/A" if value is below 0.
    /// </summary>
    public class DoubleToStringConverter : IValueConverter
    {
        private static (double, bool) ConvertValue(object value)
        {
            // Allow some other convertible num types
            switch (value)
            {
                case double d: return (d, true);
                case float f: return (f, true);
                case int i: return (i, true);
                default: return (0, false);
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var (d, ok) = ConvertValue(value);
            if (!ok) return DependencyProperty.UnsetValue;
            return d < 0 ? Translations.Tr("N/A") : d.ToString("F0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
