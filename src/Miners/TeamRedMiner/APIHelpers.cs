using Newtonsoft.Json;
using NHM.Common;
using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TeamRedMiner
{
    public static class APIHelpers
    {
        const string jsonDevsApiCall = "{\"command\": \"devs\"}";

        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Culture = CultureInfo.InvariantCulture
        };

        public static ApiDevsRoot ParseApiDevsRoot(string respStr)
        {
            var resp = JsonConvert.DeserializeObject<ApiDevsRoot>(respStr, _jsonSettings);
            return resp;
        }

        public static async Task<(ApiDevsRoot root, string response)> GetApiDevsRootAsync(int port, string logGroup)
        {
            try
            {
                using (var client = new TcpClient("127.0.0.1", port))
                using (var nwStream = client.GetStream())
                {
                    var bytesToSend = Encoding.ASCII.GetBytes(jsonDevsApiCall);
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    client.Close();
                    var resp = JsonConvert.DeserializeObject<ApiDevsRoot>(respStr, _jsonSettings);
                    return (resp, respStr);
                }
            }
            catch (Exception e)
            {
                Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                return (null, null);
            }
        }
    }

}
