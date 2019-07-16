using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        #region TODO temporary IRatesComunication / refactoring
        // TODO temporary
        public static IRatesComunication _ratesComunication = null; // for now should only have one of these

        public static void ClearRatesAll()
        {
            _ratesComunication?.ClearRatesAll();
        }

        public static void RefreshRates()
        {
            _ratesComunication?.RefreshRates();
        }
        #endregion

        #region Set Mining Profitable or NOT Profitable
        //// TODO we got a problem here with displaying and sending IFTTT stuff, since we can start/stop 
        //private static bool isProfitable = false;
        // TODO IFTTT might be broken or it may spam with messages
        public static void SetProfitableState(bool isProfitable)
        {
            if (isProfitable)
            {
                if (ConfigManager.GeneralConfig.UseIFTTT)
                {
                    Ifttt.PostToIfttt("nicehash", "Mining is once again profitable and has resumed.");
                }
                DisplayMiningProfitable(isProfitable);
            } else
            {
                if (ConfigManager.GeneralConfig.UseIFTTT)
                {
                    Ifttt.PostToIfttt("nicehash", "CURRENTLY MINING NOT PROFITABLE.");
                }
                DisplayMiningProfitable(isProfitable);
            }
        }
        #endregion

        public static void DisplayTotalRate(double totalMiningRate)
        {
            DisplayGlobalMiningRate?.Invoke(null, totalMiningRate);
        }

        public static void DisplayMiningProfitable(bool isProfitable)
        {
            _DisplayMiningProfitability?.Invoke(null, isProfitable);
        }

        public static void DisplayNoInternetConnection(bool noInternet)
        {
            _DisplayNoInternetConnection?.Invoke(null, noInternet);
        }
    }
}
