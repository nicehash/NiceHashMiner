using NHM.MinerPlugin;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
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

        private static int _acceptedShares;
        private static int _rejectedShares;
        private static DateTime _lastAcceptedShare;
        private static DateTime _lastRejectedShare;

        public static async Task<ApiData> GetMinerStatsDataAsync(int apiPort, IReadOnlyList<BaseDevice> miningDevices, string logGroup, double DevFee, double DualDevFee, params AlgorithmType[] algorithmTypes)
        {
            var ads = new ApiDataShare(new ApiData());

            var firstAlgoType = AlgorithmType.NONE;
            var secondAlgoType = AlgorithmType.NONE;

            bool isDual = algorithmTypes.Count() > 1;

            if (algorithmTypes.Count() > 0) firstAlgoType = algorithmTypes[0];
            if (algorithmTypes.Count() > 1) secondAlgoType = algorithmTypes[1];

            var totalSpeed = new List<(AlgorithmType type, double speed)>();
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();

            var perDeviceAcceptedShareInfo = new Dictionary<string, (int, DateTime)>();
            var perDeviceRejectedShareInfo = new Dictionary<string, (int, DateTime)>();

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
                    ads.ApiResponse = respStr;
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

                        var shares = TransformSpeedsList(resp.result[2]);
                        if (shares[1] != _acceptedShares)
                        {
                            _acceptedShares = Convert.ToInt32(shares[1]);
                            _lastAcceptedShare = DateTime.Now;
                        }
                        if (shares[2] != _rejectedShares)
                        {
                            _rejectedShares = Convert.ToInt32(shares[2]);
                            _lastRejectedShare = DateTime.Now;
                        }

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
                            perDeviceSpeedInfo.Add(uuid, perDeviceSpeeds );
                            // no power usage info
                            perDevicePowerInfo.Add(uuid, -1);

                            perDeviceAcceptedShareInfo.Add(uuid, (_acceptedShares, _lastAcceptedShare));
                            perDeviceRejectedShareInfo.Add(uuid, (_rejectedShares, _lastRejectedShare));
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
            ads.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ads.PowerUsagePerDevice = perDevicePowerInfo;
            ads.PowerUsageTotal = -1;

            ads.AcceptedShareInfoPerDevice = perDeviceAcceptedShareInfo;
            ads.RejectedShareInfoPerDevice = perDeviceRejectedShareInfo;
            return ads;
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
