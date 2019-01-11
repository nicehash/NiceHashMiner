using NiceHashMiner.Miners;

namespace NiceHashMiner.Interfaces
{
    public interface IRatesComunication
    {
        void ClearRatesAll();

        void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException);

        // The following four must use an invoker since they may be called from non-UI thread
        
        void ShowNotProfitable(string msg);

        void HideNotProfitable();

        void ForceMinerStatsUpdate();
    }
}
