// TESTNET
#if TESTNET || TESTNETDEV
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Miners;

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

        //public static void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException)
        //{
        //    _ratesComunication?.AddRateInfo(iApiData, paying, isApiGetException);
        //}

        // The following four must use an invoker since they may be called from non-UI thread

        //public static void ShowNotProfitable(string msg)
        //{
        //    _ratesComunication?.ShowNotProfitable(msg);
        //}

        //public static void HideNotProfitable()
        //{
        //    _ratesComunication?.HideNotProfitable();
        //}
        #endregion

        // 
        public static void ShowNoInternetConnection(string msg)
        {
        }

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
                DisplayMiningProfitable();
            } else
            {
                if (ConfigManager.GeneralConfig.UseIFTTT)
                {
                    Ifttt.PostToIfttt("nicehash", "CURRENTLY MINING NOT PROFITABLE.");
                }
                DisplayMiningNotProfitable();
            }
        }
        #endregion

        public static void DisplayTotalRate(double totalMiningRate)
        {
            DisplayGlobalMiningRate?.Invoke(null, totalMiningRate);
        }

        public static void DisplayMiningNotProfitable()
        {
            //ShowNotProfitable(Translations.Tr("CURRENTLY MINING NOT PROFITABLE."));
            _DisplayMiningNotProfitable?.Invoke(null, null);
        }

        public static void DisplayMiningProfitable()
        {
            _DisplayMiningProfitable?.Invoke(null, null);
        }

        public static void DisplayNoInternetConnection()
        {
            //ShowNotProfitable(Translations.Tr("CURRENTLY NOT MINING. NO INTERNET CONNECTION.")); 
            _DisplayNoInternetConnection?.Invoke(null, null);
        }
    }
}
#endif
