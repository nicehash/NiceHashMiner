using NiceHashMiner.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NHMCore;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NiceHashMiner.Views.ParameterOverview
{
    class FlagOrValueHeaderStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceELPData devELP)
            {
                if(devELP.IsDeviceDataHeader) return Translations.Tr("Flag");
                return Translations.Tr("Value");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
