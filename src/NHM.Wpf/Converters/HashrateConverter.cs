using System.Collections.Generic;
using NHM.Common;

namespace NHM.Wpf.Converters
{
    public class HashrateConverter : ConverterBase<IEnumerable<Hashrate>, string>
    {
        public override string Convert(IEnumerable<Hashrate> value, string parameter)
        {
            return string.Join(" + ", value);
        }
    }
}
