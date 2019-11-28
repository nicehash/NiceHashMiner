using NHM.Common;
using NHMCore.Configs;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace NHMCore
{
    public class Ifttt
    {
        private const string ApiUrl = "https://maker.ifttt.com/trigger/";

        public static void PostToIfttt(string action, string msg)
        {
            try
            {
                var key = IFTTTSettings.Instance.IFTTTKey;
                var worker = CredentialsSettings.Instance.WorkerName;
                var minProfit = MiningProfitSettings.Instance.MinimumProfit.ToString("F2").Replace(',', '.');

                using (var client = new WebClient())
                {
                    var postData = new NameValueCollection
                    {
                        ["value1"] = worker,
                        ["value2"] = msg,
                        ["value3"] = minProfit
                    };

                    var response = client.UploadValues(ApiUrl + action + "/with/key/" + key, postData);

                    var responseString = Encoding.Default.GetString(response);
                }
            }
            catch (Exception ex)
            {
                Logger.Info("Ifttt", $"Error occured: {ex.Message}");
            }
        }
    }
}
