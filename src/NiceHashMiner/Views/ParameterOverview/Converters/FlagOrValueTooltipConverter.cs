using NHMCore;
using NHMCore.Configs.ELPDataModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NiceHashMiner.Views.ParameterOverview
{
    class FlagOrValueTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HeaderType ht)
            {
                if (ht == HeaderType.Value) return Translations.Tr("Value part of this specific device, usually a number");
                return Translations.Tr("Flag and delimiter separated by a single space, for example '--exampleFlag ,'");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
