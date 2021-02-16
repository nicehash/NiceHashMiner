using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.ClaymoreCommon
{
    public static class ClaymoreAPIHelpers
    {
        const string _jsonStatsApiCall = "{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}\n";

        private static readonly List<double> _emptySpeeds = new List<double>();

        public static async Task<ApiData> GetMinerStatsDataAsync(int apiPort, IReadOnlyList<BaseDevice> miningDevices, string logGroup, double DevFee, double DualDevFee, params AlgorithmType[] algorithmTypes)
        {
            var ad = new ApiData();

            var firstAlgoType = AlgorithmType.NONE;
            var secondAlgoType = AlgorithmType.NONE;

            bool isDual = algorithmTypes.Count() > 1;

            if (algorithmTypes.Count() > 0) firstAlgoType = algorithmTypes[0];
            if (algorithmTypes.Count() > 1) secondAlgoType = algorithmTypes[1];

            var totalSpeed = new List<(AlgorithmType type, double speed)>();
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();

            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes(_jsonStatsApiCall);
                using (var client = new TcpClient("127.0.0.1", apiPort))
                using (var nwStream = client.GetStream())
                {
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    ad.ApiResponse = respStr;
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr);
                }
                if (resp != null && resp.error == null)
                {
                    if (resp.result != null && resp.result.Count > 3)
                    {
                        var speeds = TransformSpeedsList(resp.result[3]);
                        var hasSecond = isDual && resp.result.Count > 5;
                        var secondarySpeeds = hasSecond ? TransformSpeedsList(resp.result[5]) : _emptySpeeds;
                        var primaryTotalSpeed = 0d;
                        var secondaryTotalSpeed = 0d;

                        for (int i = 0; i < miningDevices.Count(); i++)
                        {
                            var dev = miningDevices[i];
                            var uuid = dev.UUID;

                            var primaryCurrentSpeed = speeds.Count > i ? speeds[i] : 0d;
                            var secondaryCurrentSpeed = secondarySpeeds.Count > i ? secondarySpeeds[i] : 0d;

                            primaryTotalSpeed += primaryCurrentSpeed;
                            secondaryTotalSpeed += secondaryCurrentSpeed;

                            var perDeviceSpeeds = new List<(AlgorithmType type, double speed)>() { (firstAlgoType, primaryCurrentSpeed * (1 - DevFee * 0.01)) };
                            if (isDual)
                            {
                                perDeviceSpeeds.Add((secondAlgoType, secondaryCurrentSpeed * (1 - DualDevFee * 0.01)));
                            }
                            perDeviceSpeedInfo.Add(uuid, perDeviceSpeeds);
                            // no power usage info
                            perDevicePowerInfo.Add(uuid, -1);
                        }

                        totalSpeed.Add((firstAlgoType, primaryTotalSpeed * (1 - DevFee * 0.01)));
                        if (isDual)
                        {
                            totalSpeed.Add((secondAlgoType, secondaryTotalSpeed * (1 - DualDevFee * 0.01)));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
            }
            ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ad.PowerUsagePerDevice = perDevicePowerInfo;
            ad.PowerUsageTotal = -1;
            return ad;
        }

        private static List<double> TransformSpeedsList(string speedsStr)
        {
            var ret = new List<double>();
            var speeds = speedsStr.Split(';');
            foreach (var speed in speeds)
            {
                double parsedSpeed = 0;
                try
                {
                    parsedSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                }
                catch
                {
                    parsedSpeed = 0;
                }
                ret.Add(parsedSpeed);
            }
            return ret;
        }
    }
}
