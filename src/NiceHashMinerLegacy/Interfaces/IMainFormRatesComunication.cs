// PRODUCTION
#if !(TESTNET || TESTNETDEV)
using NiceHashMiner.Miners;

namespace NiceHashMiner.Interfaces
{
    public interface IMainFormRatesComunication
    {
        void ClearRatesAll();

        //void RaiseAlertSharesNotAccepted(string algoName);
        void RefreshRates();

        // The following four must use an invoker since they may be called from non-UI thread
        
        void ShowNotProfitable(string msg);

        void HideNotProfitable();
    }
}
#endif
