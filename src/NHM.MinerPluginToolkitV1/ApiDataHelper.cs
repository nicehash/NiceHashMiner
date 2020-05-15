using NHM.Common;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1
{
    // this is temporary since we know it alredy works, try not to use it or replace it later
    public class ApiDataHelpers
    {
        public static string GetHttpRequestNhmAgentString(string cmd)
        {
            return "GET /" + cmd + " HTTP/1.1\r\n" +
                   "Host: 127.0.0.1\r\n" +
                   "User-Agent: NiceHashMinerPlugin/0.0.0.1" + "\r\n" +
                   "\r\n";
        }

        public static async Task<string> GetApiDataAsync(int port, string dataToSend, string logGroup)
        {
            try
            {
                using (var client = new TcpClient("127.0.0.1", port))
                using (var nwStream = client.GetStream())
                {
                    var bytesToSend = Encoding.ASCII.GetBytes(dataToSend);
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    client.Close();
                    return respStr;
                }
            }
            catch (Exception e)
            {
                Logger.Error(logGroup, $"Error occured while getting api data base: {e.Message}");
                return "";
            }
        }
    }
}
