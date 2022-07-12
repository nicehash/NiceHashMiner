using NiceHashMiner.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NHMCore;
using System.Threading.Tasks;
using System.Windows.Data;
using NHMCore.Configs.ELPDataModels;

namespace NiceHashMiner.Views.ParameterOverview
{
    class FlagOrValueHeaderEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HeaderType ht)
            {
                if(ht == HeaderType.Value) return Translations.Tr("Value");
                return Translations.Tr("Flag & delimiter");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
