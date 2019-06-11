using MinerPlugin;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Enums;
using System.Text;
using NiceHashMinerLegacy.Common;

namespace MinerPluginToolkitV1.SgminerCommon
{
    public static class SgminerAPIHelpers
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

        public static async Task<ApiDevsRoot> GetApiDevsRootAsync(int port, string logGroup)
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
                    return resp;
                }
            }
            catch (Exception e)
            {
                Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                return null;
            }
        }

        // TODO implement if all devices alive function check

        public static ApiData ParseApiDataFromApiDevsRoot(ApiDevsRoot apiDevsResult, AlgorithmType algorithmType, IEnumerable<BaseDevice> miningDevices, string logGroup)
        {
            var ad = new ApiData();
            if (apiDevsResult == null) return ad;

            try
            {
                var deviveStats = apiDevsResult.DEVS;
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                foreach (var gpu in miningDevices)
                {
                    var deviceStats = deviveStats
                        .Where(devStat => gpu.ID == devStat.GPU)
                        .FirstOrDefault();
                    if (deviceStats == null)
                    {
                        Logger.Info(logGroup, $"Device stats from api data are empty. Device: {gpu.UUID}");
                        continue;
                    }
                        

                    var speedHS = deviceStats.KHS_5s * 1000;
                    totalSpeed += speedHS;
                    var algoSpeedPair = new AlgorithmTypeSpeedPair(algorithmType, speedHS);
                    perDeviceSpeedInfo.Add(gpu.UUID, new List<AlgorithmTypeSpeedPair>() { algoSpeedPair });
                    // TODO check PowerUsage API
                }
                var totalAlgoSpeedPair = new AlgorithmTypeSpeedPair(algorithmType, totalSpeed);
                ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair>() { totalAlgoSpeedPair };
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;

                ad.PowerUsagePerDevice = perDevicePowerInfo;
                ad.PowerUsageTotal = totalPowerUsage;

            }
            catch (Exception e)
            {
                Logger.Error(logGroup, $"Error occured while parsing API stats: {e.Message}");
            }

            return ad;
        }
    }
}
