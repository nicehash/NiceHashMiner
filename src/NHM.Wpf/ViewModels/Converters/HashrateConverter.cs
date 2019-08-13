using System.Collections.Generic;
using NHM.Common;
using NHM.Common.Enums;
using NiceHashMiner.Utils;

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
