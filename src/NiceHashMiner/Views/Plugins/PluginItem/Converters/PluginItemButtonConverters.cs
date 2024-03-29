﻿using NHMCore.Mining.Plugins;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NiceHashMiner.Views.Plugins.PluginItem.Converters
{
    //class PluginItemButtonConverters
    //{
    //}

    public class PluginItemButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var style = Application.Current.FindResource("ButtonPluginInstall");
            if (value is PluginPackageInfoCR pluginCR)
            {
                if (pluginCR.Installed) style = Application.Current.FindResource("ButtonPluginRemove");
                if (!AcceptedPlugins.IsAccepted(pluginCR.PluginUUID) && pluginCR.Installed) style = Application.Current.FindResource("ButtonAcceptTOS");
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
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
