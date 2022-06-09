using Newtonsoft.Json;
using NHM.Common;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class QrCodeGenerator
    {
        internal class RigUUIDRequest
        {
            public string qrId { get; set; }
            public string rigId { get; set; }
        }

        internal class BtcResponse
        {
            public string btc { get; set; }
        }

        public static async Task<bool> RequestNew_QR_Code(string uuid, string rigId)
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(new RigUUIDRequest { qrId = uuid, rigId = rigId });
                using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                using var client = new HttpClient();
                var response = await client.PostAsync("https://api2.nicehash.com/api/v2/organization/nhmqr", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Logger.Error("QrCodeGenerator", $"RequestNew_QR_Code Got Exception: {e.Message}");
                return false;
            }
        }

        public static async Task<string> GetBTCForUUID(string uuid)
        {
            try
            {
                using var client = new HttpClient();
                using var resp = await client.GetAsync($"https://api2.nicehash.com/api/v2/organization/nhmqr/{uuid}");
                if (!resp.IsSuccessStatusCode) {
                    Logger.Warn("QrCodeGenerator", $"GetBTCForUUID IsSuccessStatusCode: {resp.IsSuccessStatusCode}");
                    return null;
                }
                var contentString = await resp.Content.ReadAsStringAsync();
                Logger.Info("QrCodeGenerator", $"GetBTCForUUID contentString {contentString}");
                if (string.IsNullOrEmpty(contentString)) {
                    return null;
                }
                var btcResp = JsonConvert.DeserializeObject<BtcResponse>(contentString);
                return btcResp?.btc ?? null;
            }
            catch (Exception e)
            {
                Logger.Error("QrCodeGenerator", $"GetBTCForUUID Got Exception: {e.Message}");
                return null;
            }
        }
    }
}
