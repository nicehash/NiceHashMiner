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
        public class Question
        {
            public string name { get; set; }
            public int type { get; set; }
        }

        public class Answer
        {
            public string name { get; set; }
            public int type { get; set; }
            public int TTL { get; set; }
            public string data { get; set; }
        }

        public class DNSReply
        {
            public int Status { get; set; }
            public bool TC { get; set; }
            public bool RD { get; set; }
            public bool RA { get; set; }
            public bool AD { get; set; }
            public bool CD { get; set; }
            public List<Question> Question { get; set; }
            public List<Answer> Answer { get; set; }
        }

        static readonly HttpClient client = new HttpClient();
        static DNSQuery()
        {
            //client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/dns-json");
        }
        private static string[] destinations =
        {
            "cloudflare-dns.com","1.1.1.1","1.0.0.1",
        };
        private static string URL = "https://{DESTINATION}/dns-query?name={REQUEST}&type={TYPE}";
        const string DESTINATION_TEMPLATE = "{DESTINATION}";
        const string REQUEST_TEMPLATE = "{REQUEST}";
        const string TYPE_TEMPLATE = "{TYPE}";


        public static async Task<string> QueryOrDefault(string url)
        {

            foreach (var dest in destinations)
            {
                var requestLocation = URL;
                requestLocation = requestLocation.Replace(DESTINATION_TEMPLATE, dest);
                requestLocation = requestLocation.Replace(REQUEST_TEMPLATE, url);
                requestLocation = requestLocation.Replace(TYPE_TEMPLATE, "A");//TODO?
                var ip = await Request(requestLocation);
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
                var parsedObject = JsonConvert.DeserializeObject<DNSReply>(responseBody);
                if (parsedObject.Answer == null) return null;
                if (parsedObject.Answer.Count == 0) return null;
                var addresses = GetAddressList(parsedObject.Answer);
                if (addresses.Count == 0) return null;
                return addresses.FirstOrDefault();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Request to " + targetUrl + " failed!");
                return null;
            }
        }

        private static List<string> GetAddressList(List<Answer> answers)
        {
            List<string> returnedAddressList = new List<string>();
            foreach (var ans in answers)
            {
                if (ans.data == null) continue;
                if (ValidateIPv4(ans.data)) returnedAddressList.Add(ans.data);
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
