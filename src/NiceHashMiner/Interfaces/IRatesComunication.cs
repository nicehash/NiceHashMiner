using NiceHashMiner.Miners;

namespace NiceHashMiner.Interfaces
{
    public interface IRatesComunication
    {
        void ClearRatesAll();

        void RefreshRates();
        //void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException);
    }
}
