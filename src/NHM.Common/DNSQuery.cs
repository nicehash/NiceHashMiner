using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Common
{
    public static class DNSQuery
    {
        static readonly HttpClient client = new HttpClient();
        static DNSQuery()
        {
            //client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/dns-json");
        }
        private static string[] urls =
        {
            "https://cloudflare-dns.com/dns-query?name=",
            "https://1.1.1.1/dns-query?name=",
            "https://1.0.0.1/dns-query?name=",
        };


        public static async Task<string> QueryOrDefault(string url)
        {

            for (int i = 0; i < 3; i++)
            {
                var ip = await Request(urls[i] + url + "&type=A");
                if (ip != null) return ip;
            }
            return url;
        }

        private static async Task<string> Request(string targetUrl)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(targetUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic parsedObject = JsonConvert.DeserializeObject(responseBody);
                var answers = parsedObject["Answer"];
                if (answers == null) return null;

                var addresses = GetAddressList(answers) as List<string>;
                if (addresses.Count == 0) return null;

                foreach (var address in addresses)
                {
                    if (ValidateIPv4(address))
                    {
                        return address;
                    }
                }
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine("Request to " + targetUrl + " failed!");
                return null;
            }
        }

        private static List<string> GetAddressList(dynamic answers)
        {
            List<string> returnedAddressList = new List<string>();
            foreach (var ans in answers)
            {
                var data = ans["data"];
                if (data != null)
                {
                    returnedAddressList.Add(data.Value);
                }
            }
            return returnedAddressList;
        }

        private static bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString)) return false;
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4) return false;
            byte parsingValues;
            return splitValues.All(r => byte.TryParse(r, out parsingValues));
        }
    }
}
