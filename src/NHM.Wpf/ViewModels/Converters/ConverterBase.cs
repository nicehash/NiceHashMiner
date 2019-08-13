using System;
using System.Globalization;
using System.Windows.Data;

namespace NHM.Wpf.ViewModels.Converters
{
    // Helper class for type-safe conversions
    public abstract class ConverterBase<TIn, TOut> : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TIn t)) return null;
            return Convert(t, parameter as string);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TOut t)) return null;
            return ConvertBack(t, parameter as string);
        }

        public abstract TOut Convert(TIn value, string parameter);
        public virtual TIn ConvertBack(TOut value, string parameter) => default;
    }
}
