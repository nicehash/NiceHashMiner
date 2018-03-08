namespace NiceHashMiner.Interfaces
{
    public interface IMainFormRatesComunication
    {
        void ClearRatesAll();
        void ClearRates(int groupCount);

        void AddRateInfo(string groupName, string deviceStringInfo, ApiData iApiData, double paying,
            bool isApiGetException);
        //void RaiseAlertSharesNotAccepted(string algoName);

        // The following three must be called with invoker since they may be called from non-UI thread
        
        void ShowNotProfitable(string msg);

        void HideNotProfitable();

        void ForceMinerStatsUpdate();
    }
}
