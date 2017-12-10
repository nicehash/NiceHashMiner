using NiceHashMiner.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

// TODO: @Allan update this

namespace NiceHashMiner {
    class ExchangeRateAPI {
        public class Result {
            public Object algorithms { get; set; }
            public Object servers { get; set; }
            public Object idealratios { get; set; }
            public List<Dictionary<string, string>> exchanges { get; set; }
            public Dictionary<string, double> exchanges_fiat { get; set; }
        }

        public class ExchangeRateJSON {
            public Result result { get; set; }
            public string method { get; set; }
        }

        const string apiUrl = "https://api.nicehash.com/api?method=nicehash.service.info";

        private static Dictionary<string, double> exchanges_fiat = null;
        private static double USD_BTC_rate = -1;
        public static string ActiveDisplayCurrency = "USD";

        private static bool ConverterActive {
            get { return ConfigManager.GeneralConfig.DisplayCurrency != "USD"; }
        }


        public static double ConvertToActiveCurrency(double amount) {
            if (!ConverterActive) {
                return amount;
            }

            // if we are still null after an update something went wrong. just use USD hopefully itll update next tick
            if (exchanges_fiat == null || ActiveDisplayCurrency == "USD") {
                // Moved logging to update for berevity 
                return amount;
            }

            //Helpers.ConsolePrint("CurrencyConverter", "Current Currency: " + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
            double usdExchangeRate = 1.0;
            if (exchanges_fiat.TryGetValue(ActiveDisplayCurrency, out usdExchangeRate))
                return amount * usdExchangeRate;
            else {
                Helpers.ConsolePrint("CurrencyConverter", "Unknown Currency Tag: " + ActiveDisplayCurrency + " falling back to USD rates");
                ActiveDisplayCurrency = "USD";
                return amount;
            }
        }

        public static double GetUSDExchangeRate() {
            if (USD_BTC_rate > 0) {
                return USD_BTC_rate;
            }
            return 0.0;
        }

        public static void UpdateAPI(string worker)
        {
            var WR = (HttpWebRequest)WebRequest.Create("https://blockchain.info/ticker");
            var Response = WR.GetResponse();
            var SS = Response.GetResponseStream();
            SS.ReadTimeout = 20 * 1000;
            var Reader = new StreamReader(SS);
            var ResponseFromServer = Reader.ReadToEnd();
            if (ResponseFromServer.Length == 0 || ResponseFromServer[0] != '{')
                throw new Exception("Not JSON!");
            Reader.Close();
            Response.Close();

            dynamic fiat_rates = JObject.Parse(ResponseFromServer);
            try
            {
                //USD_BTC_rate = Helpers.ParseDouble((string)fiat_rates[ConfigManager.GeneralConfig.DisplayCurrency]["last"]);
                USD_BTC_rate = Helpers.ParseDouble((string)fiat_rates["USD"]["last"]);

                exchanges_fiat = new Dictionary<string, double>();
                foreach (var c in _supportedCurrencies)
                    exchanges_fiat.Add(c, Helpers.ParseDouble((string)fiat_rates[c]["last"]) / USD_BTC_rate);
            }
            catch
            {
            }
        }

        private static readonly string[] _supportedCurrencies = {
            "AUD", 
            "BRL", 
            "CAD", 
            "CHF", 
            "CLP", 
            "CNY", 
            "DKK", 
            "EUR", 
            "GBP", 
            "HKD", 
            "INR", 
            "ISK", 
            "JPY", 
            "KRW", 
            "NZD", 
            "PLN", 
            "RUB", 
            "SEK", 
            "SGD", 
            "THB", 
            "TWD", 
            "USD"
        };
    }
}
