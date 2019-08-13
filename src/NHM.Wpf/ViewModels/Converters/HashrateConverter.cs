using NHM.Common.Enums;
using NiceHashMiner.Utils;

namespace NHM.Wpf.ViewModels.Converters
{
    public class HashrateConverter : ConverterBase<double, string>
    {
        public override string Convert(double value, string parameter)
        {
            return Helpers.FormatSpeedOutput(value, AlgorithmType.NONE);
        }
    }
}
