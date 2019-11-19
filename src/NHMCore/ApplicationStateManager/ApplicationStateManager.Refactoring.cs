using NHMCore.Configs;
using NHMCore.Interfaces;
using NHMCore.Mining;
using System;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        public static event EventHandler OnExchangeUpdate;
        public static void OnExchangeUpdated()
        {
            OnExchangeUpdate?.Invoke(null, EventArgs.Empty);
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
