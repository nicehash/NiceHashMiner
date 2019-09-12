
namespace NHMCore.Interfaces.DataVisualizer
{
    public interface INoInternetConnectionDisplayer : IDataVisualizer
    {
        void DisplayNoInternetConnection(object sender, bool noInternet);
    }
}
