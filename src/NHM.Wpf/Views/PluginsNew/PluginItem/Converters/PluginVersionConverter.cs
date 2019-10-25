using System;
using System.Globalization;
using System.Windows.Data;
using NHMCore;
using NHMCore.Mining.Plugins;

namespace NHM.Wpf.Views.PluginsNew.PluginItem.Converters
{
    public class PluginVersionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PluginPackageInfoCR pluginCR)
            {
                var localVer = pluginCR?.LocalInfo?.PluginVersion ?? null;
                var onlineVer = pluginCR?.OnlineInfo?.PluginVersion ?? null;
                if (!pluginCR.Installed && onlineVer != null)
                {
                    return $"{onlineVer.Major}.{onlineVer.Minor} (Online)";
                }
                if (pluginCR.Installed && pluginCR.HasNewerVersion && localVer != null && onlineVer != null)
                {
                    return $"{localVer.Major}.{localVer.Minor} / {onlineVer.Major}.{onlineVer.Minor}";
                }
                if (pluginCR.Installed && !pluginCR.HasNewerVersion && localVer != null)
                {
                    // TODO translate
                    return $"{localVer.Major}.{localVer.Minor} (Latest)";
                }
                if (localVer != null)
                {
                    // TODO Tranlsate
                    return $"{localVer.Major}.{localVer.Minor} (Local)";
                }
            }
            return Translations.Tr("N/A");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
