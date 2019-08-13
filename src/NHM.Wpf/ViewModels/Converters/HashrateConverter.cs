using System.Collections.Generic;
using NHM.Common;
using NHM.Common.Enums;
using NiceHashMiner.Utils;

namespace NHM.Wpf.ViewModels.Converters
{
    public class HashrateConverter : ConverterBase<IReadOnlyList<Hashrate>, string>
    {
        public override string Convert(IReadOnlyList<Hashrate> value, string parameter)
        {
            return string.Join(" + ", value);
        }
    }
}
