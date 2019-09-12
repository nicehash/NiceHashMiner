
namespace NHMCore.Interfaces.DataVisualizer
{
    public interface IBalanceBTCDisplayer : IDataVisualizer
    {
        void DisplayBTCBalance(object sender, double btcBalance);
    }
}
