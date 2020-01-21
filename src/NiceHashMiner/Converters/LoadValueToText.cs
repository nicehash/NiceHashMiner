using System;
using System.Globalization;
using System.Windows.Data;

namespace NiceHashMiner.Converters
{
    class LoadValueToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return Translations.Tr(value);
            if (value is float load && load > -1)
            {
                return $"{load:F0}%";
            }
            return "- - -";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
