using Newtonsoft.Json;
using NiceHashMiner.Configs;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NiceHashMiner
{
    class IFTTT
    {
        public class Result
        {
            public Object algorithms { get; set; }
            public Object servers { get; set; }
            public Object idealratios { get; set; }
            public List<Dictionary<string, string>> exchanges { get; set; }
            public Dictionary<string, double> exchanges_fiat { get; set; }
        }

        public class ExchangeRateJSON
        {
            public Result result { get; set; }
            public string method { get; set; }
        }

        const string apiUrl = "https://maker.ifttt.com/trigger/";

        private static Dictionary<string, double> exchanges_fiat = null;
        private static double USD_BTC_rate = -1;
        public static string ActiveDisplayCurrency = "USD";

        private static bool ConverterActive
        {
            get { return ConfigManager.GeneralConfig.DisplayCurrency != "USD"; }
        }


        public static double ConvertToActiveCurrency(double amount)
        {
            if (!ConverterActive)
            {
                return amount;
            }

            // if we are still null after an update something went wrong. just use USD hopefully itll update next tick
            if (exchanges_fiat == null || ActiveDisplayCurrency == "USD")
            {
                Helpers.ConsolePrint("CurrencyConverter", "Unable to retrieve update, Falling back to USD");
                return amount;
            }

            //Helpers.ConsolePrint("CurrencyConverter", "Current Currency: " + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
            double usdExchangeRate = 1.0;
            if (exchanges_fiat.TryGetValue(ActiveDisplayCurrency, out usdExchangeRate))
                return amount * usdExchangeRate;
            else
            {
                Helpers.ConsolePrint("CurrencyConverter", "Unknown Currency Tag: " + ActiveDisplayCurrency + " falling back to USD rates");
                ActiveDisplayCurrency = "USD";
                return amount;
            }
        }

        public static double GetUSDExchangeRate()
        {
            if (USD_BTC_rate > 0)
            {
                return USD_BTC_rate;
            }
            return 0.0;
        }

        public static void UpdateAPI(string worker)
        {
            string resp = NiceHashStats.GetNiceHashAPIData(apiUrl, worker);
            if (resp != null)
            {
                try
                {
                    var LastResponse = JsonConvert.DeserializeObject<ExchangeRateJSON>(resp, Globals.JsonSettings);
                    // set that we have a response
                    if (LastResponse != null)
                    {
                        Result last_result = LastResponse.result;
                        ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;
                        exchanges_fiat = last_result.exchanges_fiat;
                        // ActiveDisplayCurrency = "USD";
                        // check if currency avaliable and fill currency list
                        foreach (var pair in last_result.exchanges)
                        {
                            if (pair.ContainsKey("USD") && pair.ContainsKey("coin") && pair["coin"] == "BTC" && pair["USD"] != null)
                            {
                                USD_BTC_rate = Helpers.ParseDouble(pair["USD"]);
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("IFTTT", "UpdateAPI got Exception: " + e.Message);
                }
            }
            else
            {
                Helpers.ConsolePrint("IFTTT", "UpdateAPI got NULL");
            }
        }

        public static string PostToIFTTT(string URL, string worker)
        {
            string ResponseFromServer;
            try
            {

                using (WebClient client = new WebClient())
                {

                    byte[] response =
                    client.UploadValues("http://dork.com/service", new NameValueCollection()
       {
           { "home", "Cosby" },
           { "favorite+flavor", "flies" }
       });

                    string result = System.Text.Encoding.UTF8.GetString(response);
                }
                //HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(URL);
                //WR.UserAgent = "NiceHashMiner/" + Application.ProductVersion;
                //if (worker.Length > 64) worker = worker.Substring(0, 64);
                //WR.Headers.Add("NiceHash-Worker-ID", worker);
                //WR.Timeout = 30 * 1000;
                //WebResponse Response = WR.GetResponse();
                //Stream SS = Response.GetResponseStream();
                //SS.ReadTimeout = 20 * 1000;
                //StreamReader Reader = new StreamReader(SS);
                //ResponseFromServer = Reader.ReadToEnd();
                //if (ResponseFromServer.Length == 0 || ResponseFromServer[0] != '{')
                //    throw new Exception("Not JSON!");
                //Reader.Close();
                //Response.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", ex.Message);
                return null;
            }

            return ResponseFromServer;
        }

    }
}
