namespace NiceHashMiner.Interfaces
{
    public interface IMainFormRatesComunication
    {
        void ClearRatesAll();
        void ClearRates(int groupCount);

        void AddRateInfo(string groupName, string deviceStringInfo, APIData iApiData, double paying,
            bool isApiGetException);

        void ShowNotProfitable(string msg);

        void HideNotProfitable();
        //void RaiseAlertSharesNotAccepted(string algoName);
    }
}
