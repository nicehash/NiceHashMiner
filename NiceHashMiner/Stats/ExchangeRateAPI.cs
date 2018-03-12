using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Stats
{
    internal static class ExchangeRateApi
    {
        private const string ApiUrl = "https://api.nicehash.com/api?method=nicehash.service.info";

        private static readonly ConcurrentDictionary<string, double> ExchangesFiat = new ConcurrentDictionary<string, double>();
        private static double _usdBtcRate = -1;

        public static double UsdBtcRate
        {
            // Access in thread-safe way
            private get => Interlocked.Exchange(ref _usdBtcRate, _usdBtcRate);
            set
            {
                Interlocked.Exchange(ref _usdBtcRate, value); 
                Helpers.ConsolePrint("NICEHASH", $"USD rate updated: {value} BTC");
            }
        }
        public static string ActiveDisplayCurrency = "USD";

        private static bool ConverterActive => ConfigManager.GeneralConfig.DisplayCurrency != "USD";

        public static void UpdateExchangesFiat(Dictionary<string, double> newExchanges)
        {
            if (newExchanges == null) return;
            foreach (var key in newExchanges.Keys)
            {
                ExchangesFiat.AddOrUpdate(key, newExchanges[key], (k, v) => newExchanges[k]);
            }
        }

        public static double ConvertToActiveCurrency(double amount)
        {
            if (!ConverterActive)
            {
                return amount;
            }

            // if we are still null after an update something went wrong. just use USD hopefully itll update next tick
            if (ExchangesFiat.Count == 0 || ActiveDisplayCurrency == "USD")
            {
                return amount;
            }

            //Helpers.ConsolePrint("CurrencyConverter", "Current Currency: " + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
            if (ExchangesFiat.TryGetValue(ActiveDisplayCurrency, out var usdExchangeRate))
                return amount * usdExchangeRate;

            Helpers.ConsolePrint("CurrencyConverter", "Unknown Currency Tag: " + ActiveDisplayCurrency + " falling back to USD rates");
            ActiveDisplayCurrency = "USD";
            return amount;
        }

        public static double GetUsdExchangeRate()
        {
            return UsdBtcRate > 0 ? UsdBtcRate : 0.0;
        }

        //[Obsolete("UpdateApi is deprecated, use websocket method")]
        //public static void UpdateApi(string worker)
        //{
        //    var resp = NiceHashStats.GetNiceHashApiData(ApiUrl, worker);
        //    if (resp != null)
        //    {
        //        try
        //        {
        //            var lastResponse = JsonConvert.DeserializeObject<ExchangeRateJson>(resp, Globals.JsonSettings);
        //            // set that we have a response
        //            if (lastResponse != null)
        //            {
        //                var lastResult = lastResponse.result;
        //                ExchangesFiat = lastResult.exchanges_fiat;
        //                if (ExchangesFiat == null)
        //                {
        //                    Helpers.ConsolePrint("CurrencyConverter", "Unable to retrieve update, Falling back to USD");
        //                    ActiveDisplayCurrency = "USD";
        //                }
        //                else
        //                {
        //                    ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;
        //                }
        //                // ActiveDisplayCurrency = "USD";
        //                // check if currency avaliable and fill currency list
        //                foreach (var pair in lastResult.exchanges)
        //                {
        //                    if (pair.ContainsKey("USD") && pair.ContainsKey("coin") && pair["coin"] == "BTC" && pair["USD"] != null)
        //                    {
        //                        UsdBtcRate = Helpers.ParseDouble(pair["USD"]);
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Helpers.ConsolePrint("ExchangeRateAPI", "UpdateAPI got Exception: " + e.Message);
        //        }
        //    }
        //    else
        //    {
        //        Helpers.ConsolePrint("ExchangeRateAPI", "UpdateAPI got NULL");
        //    }
        //}
    }
}
