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
       
        const string apiUrl = "https://maker.ifttt.com/trigger/";


        public static void PostToIFTTT(string URL, string action)
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
