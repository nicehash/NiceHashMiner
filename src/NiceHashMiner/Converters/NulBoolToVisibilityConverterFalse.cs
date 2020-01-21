using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Converters
{
    // inverse of the other thing
    public class NulBoolToVisibilityConverterFalse : IValueConverter
    {
        /// <summary>
        /// Convert an object to visibility, with the basis that null is Visbility.Collapsed.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="parameter">String "1" to invert the answer</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Explicit XAML parameters will come in as strings, here we say if it is a number > 0 
            // then we invert (show the opposite visibility)
            var b = (bool)value;

            // XOR will use the opposite answer if invert is true
            var ret = !b ? Visibility.Visible : Visibility.Collapsed;
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
