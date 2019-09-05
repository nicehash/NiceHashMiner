using NHM.Common;
using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Converters
{
    public class HashrateConverter : ConverterBase<IEnumerable<Hashrate>, string>
    {
        public override string Convert(IEnumerable<Hashrate> value, string parameter)
        {
            return string.Join(" + ", value);
        }
    }
}
