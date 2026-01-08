using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Settings
{
    // inverse of the other thing
    public class BoolInvert : IValueConverter
    {
        /// <summary>
        /// Convert an object to visibility, with the basis that null is Visbility.Collapsed.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="parameter">String "1" to invert the answer</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            return !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
