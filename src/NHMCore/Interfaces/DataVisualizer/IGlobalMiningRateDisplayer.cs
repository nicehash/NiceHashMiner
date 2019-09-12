
namespace NHMCore.Interfaces.DataVisualizer
{
    // TODO rename to total?
    public interface IGlobalMiningRateDisplayer : IDataVisualizer
    {
        void DisplayGlobalMiningRate(object sender, double totalMiningRate);
    }
}
