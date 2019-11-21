using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NHMCore;
using NHMCore.Mining.Plugins;

namespace NHM.Wpf.Views.Plugins.PluginItem.Converters
{
    //class PluginItemButtonConverters
    //{
    //}

    public class PluginItemButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var style = Application.Current.FindResource("BigButtonPrimary");
            if (value is bool installed && installed)
            {
                return Application.Current.FindResource("BigButtonWhite");
            }
            return style;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PluginItemVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool installed && installed)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
