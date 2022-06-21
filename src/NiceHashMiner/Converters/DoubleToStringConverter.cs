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
        // Allow some other convertible num types
        private static (bool ok, double num) ConvertValue(object value) => value switch
        {
            double d => (true, d),
            float f => (true, f),
            int i => (true, i),
            _ => (false, 0),
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertValue(value) switch
            {
                (false, _) => DependencyProperty.UnsetValue,
                (true, > 0) r => r.num.ToString("F0"),
                _ => Translations.Tr("N/A"),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
