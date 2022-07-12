using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NiceHashMiner.Converters
{
    class BooleanDaysToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Dictionary<string, bool>)
            {
                var days = new List<string>();
                foreach (var day in value as Dictionary<string, bool>)
                {
                    if (day.Value) days.Add(day.Key.ToUpper().Substring(0,3));
                }
                if (!days.Any()) return "-";

                return $"{string.Join(',', days)}";
            }

            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
