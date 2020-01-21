using System;
using System.Globalization;
using System.Windows.Data;
using NHMCore;

namespace NiceHashMiner.Converters
{
    public class TranslatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Translations.Tr(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
