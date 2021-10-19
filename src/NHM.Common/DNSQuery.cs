using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NHM.Common
{
    public static class DNSQuery
    {
        #region JSON DTOs
#pragma warning disable IDE1006 // Naming Styles
        private class Answer
        {
            public string data { get; set; }
        }

        private class DNSReply
        {
            public List<Answer> Answer { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles
        #endregion JSON DTOs

        private static readonly HttpClient _client = new HttpClient();

        private static readonly IReadOnlyList<string> _destinations = new string[]
        {
            "cloudflare-dns.com", "1.1.1.1", "1.0.0.1",
        };

        static DNSQuery()
        {
            _client.DefaultRequestHeaders.Add("Accept", "application/dns-json");
        }

        const string URL = "https://{DESTINATION}/dns-query?name={REQUEST}&type=A";
        const string DESTINATION_TEMPLATE = "{DESTINATION}";
        const string REQUEST_TEMPLATE = "{REQUEST}";

        public static async Task<(string IP, bool gotIP)> QueryOrDefault(string url)
        {
            foreach (var dest in _destinations)
            {
                var requestLocation = URL
                    .Replace(DESTINATION_TEMPLATE, dest)
                    .Replace(REQUEST_TEMPLATE, url);
                var ip = await Request(requestLocation);
                if (ip != null) return (ip, true);
            }
            return (url, false);
        }

        private static async Task<string> Request(string targetUrl)
        {
            try
            {
                var response = await _client.GetStringAsync(targetUrl);
                var parsedObject = JsonConvert.DeserializeObject<DNSReply>(response);
                var ips = parsedObject?.Answer?
                    .Select(answer => answer.data)
                    .Where(ipString => IPAddress.TryParse(ipString, out var _));
                return ips.FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.Error("DNSQuery", $"Request to {targetUrl} failed with Error: {e.Message}");
                return null;
            }
        }
    }
}
