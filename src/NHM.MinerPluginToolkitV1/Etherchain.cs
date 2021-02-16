using Newtonsoft.Json.Linq;
using NHM.Common;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1
{
    public static class Etherchain
    {
        public static async Task<string> GetCurrentBlockAsync()
        {
            const string url = "https://etherchain.org/api/blocks/count";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "NiceHashMiner";
                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = 30 * 1000;
                using (var response = await request.GetResponseAsync())
                using (var responseStream = response.GetResponseStream())
                using (var sr = new StreamReader(responseStream))
                {
                    //Need to return this response 
                    string strContent = await sr.ReadToEndAsync();
                    var json = JObject.Parse(strContent);
                    var currentBlockNum = json.GetValue("count").ToString();
                    return currentBlockNum;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Etherchain", $"Error occured while getting current block async: {e.Message}");
                return null;
            }
        }
    }
}
