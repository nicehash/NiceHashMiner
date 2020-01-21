using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NiceHashMiner.Views.Plugins.PluginItem.Converters
{
    public class SupportedDeviceAlgorithmsConverter : IValueConverter
    {
        public class SupportedDeviceAlgorithm
        {
            public string DeviceType { get; set; }
            public List<string> AlgorithmTypes { get; set; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Dictionary<string, List<string>> supportedAlgos)
            {
                var ret = supportedAlgos.Select(kvp => new SupportedDeviceAlgorithm { DeviceType = kvp.Key, AlgorithmTypes = kvp.Value });
                return ret.ToArray();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
