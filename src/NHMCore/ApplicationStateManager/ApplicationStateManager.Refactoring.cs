using NHMCore.Configs;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        #region Set Mining Profitable or NOT Profitable
        //// TODO we got a problem here with displaying and sending IFTTT stuff, since we can start/stop 
        //private static bool isProfitable = false;
        // TODO IFTTT might be broken or it may spam with messages
        public static void SetProfitableState(bool isProfitable)
        {
            if (isProfitable)
            {
                if (IFTTTSettings.Instance.UseIFTTT)
                {
                    Ifttt.PostToIfttt("nicehash", "Mining is once again profitable and has resumed.");
                }
                DisplayMiningProfitable(isProfitable);
            }
            else
            {
                if (IFTTTSettings.Instance.UseIFTTT)
                {
                    Ifttt.PostToIfttt("nicehash", "CURRENTLY MINING NOT PROFITABLE.");
                }
                DisplayMiningProfitable(isProfitable);
            }
        }
        #endregion

        // TODO put in mining profit state 
        public static void DisplayTotalRate(double totalMiningRate)
        {
            //DisplayGlobalMiningRate?.Invoke(null, totalMiningRate);
        }
        // TODO put in mining profit state
        public static void DisplayMiningProfitable(bool isProfitable)
        {
            //_DisplayMiningProfitability?.Invoke(null, isProfitable);
        }
        // TODO put in app state
        public static void DisplayNoInternetConnection(bool noInternet)
        {
            //_DisplayNoInternetConnection?.Invoke(null, noInternet);
        }
    }
}
