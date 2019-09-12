
namespace NHMCore.Interfaces.DataVisualizer
{
    public interface IBalanceFiatDisplayer : IDataVisualizer
    {
        void DisplayFiatBalance(object sender, (double fiatBalance, string fiatCurrencySymbol) args);
    }
}
