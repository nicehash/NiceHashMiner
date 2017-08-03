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

        public static void PostToIFTTT(string action, string msg)
        {
            try
            {
                string key = ConfigManager.GeneralConfig.IFTTTKey;

                using (WebClient client = new WebClient())
                {
                    byte[] response =
                    client.UploadValues(apiUrl + action + "/with/key/" + key, new NameValueCollection()
                    {
                        { "Value1", msg }
                    });

                    string result = System.Text.Encoding.UTF8.GetString(response);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", ex.Message);
            }

        }

    }
}
