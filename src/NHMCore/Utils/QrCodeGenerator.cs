using Newtonsoft.Json;
using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class QrCodeGenerator
    {
        public static async Task<bool> RequestNew_QR_Code(string uuid, string rigId)
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(new RigUUIDRequest { qrId = uuid, rigId = rigId });
                using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync("https://api2.nicehash.com/api/v2/organization/nhmqr", content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                Logger.Error("QrCodeGenerator", $"Got Exception: {e.Message}");
                return false;
            }
        }
    }

    internal class RigUUIDRequest
    {
        public string qrId { get; set; }
        public string rigId { get; set; }
    }
}
