using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NHMCore.Mining.Plugins;

namespace NiceHashMiner.Views.Plugins.PluginItem.Converters
{
    public class CompatibleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PluginPackageInfoCR pluginCR)
            {
                //if (!pluginCR.CompatibleNHPluginVersion) return Visibility.Hidden; // show wrappanel even if not NH compatible
                if (!pluginCR.Supported) return Visibility.Hidden;
                return Visibility.Visible;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
