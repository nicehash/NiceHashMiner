using System;
using System.Globalization;
using System.Windows.Data;

namespace NHM.Wpf.ViewModels.Converters
{
    public class NulBoolToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return true;
            if (value is bool b) return b;

            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
