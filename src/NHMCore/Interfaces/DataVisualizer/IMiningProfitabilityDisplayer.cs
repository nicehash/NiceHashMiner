
namespace NHMCore.Interfaces.DataVisualizer
{
    public interface IMiningProfitabilityDisplayer : IDataVisualizer
    {
        void DisplayMiningProfitable(object sender, bool isProfitable);
    }
}
