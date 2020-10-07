using NHM.MinerPlugin;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.CCMinerCommon
{
    public static class CCMinerAPIHelpers
    {
        private static Dictionary<int, int> _acceptedSharesPerDevice = new Dictionary<int, int>();
        private static Dictionary<int, int> _rejectedSharesPerDevice = new Dictionary<int, int>();
        private static Dictionary<int, DateTime> _lastAcceptedSharePerDevice = new Dictionary<int, DateTime>();
        private static Dictionary<int, DateTime> _lastRejectedSharePerDevice = new Dictionary<int, DateTime>();

        public static Task<string> GetApiDataSummary(int port, string logGroup)
        {
            var dataToSend = ApiDataHelpers.GetHttpRequestNhmAgentString("summary");
            return ApiDataHelpers.GetApiDataAsync(port, dataToSend, logGroup);
        }

        public static Task<string> GetApiDataThreads(int port, string logGroup)
        {
            var dataToSend = ApiDataHelpers.GetHttpRequestNhmAgentString("threads");
            return ApiDataHelpers.GetApiDataAsync(port, dataToSend, logGroup);
        }

        private struct IdPowerHash
        {
            public int id;
            public int power;
            public double speed;
            public int accepted;
            public int rejected;
            public DateTime lastAccepted;
            public DateTime lastRejected;
        }
       

        public static async Task<ApiData> GetMinerStatsDataAsync(int port, AlgorithmType algorithmType, IEnumerable<MiningPair> miningPairs, string logGroup, double devFee)
        {
            var summaryApiResult = await GetApiDataSummary(port, logGroup);
            double totalSpeed = 0;
            int totalPower = 0;
            if (!string.IsNullOrEmpty(summaryApiResult))
            {
                try
                {
                    var summaryOptvals = summaryApiResult.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var optvalPairs in summaryOptvals)
                    {
                        var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pair.Length != 2) continue;
                        if (pair[0] == "KHS")
                        {
                            totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                        }
                    }
                }
                catch(Exception e)
                {
                    Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                }
            }
            var threadsApiResult = await GetApiDataThreads(port, logGroup);
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();
            var perDeviceAcceptedShareInfo = new Dictionary<string, (int, DateTime)>();
            var perDeviceRejectedShareInfo = new Dictionary<string, (int, DateTime)>();

            if (!string.IsNullOrEmpty(threadsApiResult))
            {
                try
                {
                    var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    var apiDevices = new List<IdPowerHash>();

                    foreach (var gpu in gpus)
                    {
                        var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var gpuData = new IdPowerHash();
                        foreach (var optvalPairs in gpuOptvalPairs)
                        {
                            var optval = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (optval.Length != 2) continue;
                            if (optval[0] == "GPU")
                            {
                                gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "POWER")
                            {
                                gpuData.power = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "KHS")
                            {
                                gpuData.speed = double.Parse(optval[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            }
                            if (optval[0] == "ACC")
                            {
                                gpuData.accepted = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                if (!_acceptedSharesPerDevice.ContainsKey(gpuData.id)) _acceptedSharesPerDevice.Add(gpuData.id, gpuData.accepted);
                                if (!_lastAcceptedSharePerDevice.ContainsKey(gpuData.id)) _lastAcceptedSharePerDevice.Add(gpuData.id, new DateTime());
                                if (_acceptedSharesPerDevice[gpuData.id] != gpuData.accepted)
                                {
                                    _acceptedSharesPerDevice[gpuData.id] = gpuData.accepted;
                                    _lastAcceptedSharePerDevice[gpuData.id] = DateTime.Now;
                                }
                                gpuData.lastAccepted = _lastAcceptedSharePerDevice[gpuData.id];
                            }
                            if (optval[0] == "REJ")
                            {
                                gpuData.rejected = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                if (!_rejectedSharesPerDevice.ContainsKey(gpuData.id)) _rejectedSharesPerDevice.Add(gpuData.id, gpuData.rejected);
                                if (!_lastRejectedSharePerDevice.ContainsKey(gpuData.id)) _lastRejectedSharePerDevice.Add(gpuData.id, new DateTime());
                                if (_rejectedSharesPerDevice[gpuData.id] != gpuData.rejected)
                                {
                                    _rejectedSharesPerDevice[gpuData.id] = gpuData.rejected;
                                    _lastRejectedSharePerDevice[gpuData.id] = DateTime.Now;
                                }
                                gpuData.lastRejected = _lastRejectedSharePerDevice[gpuData.id];
                            }
                        }
                        
                        apiDevices.Add(gpuData);
                    }

                    foreach (var miningPair in miningPairs)
                    {
                        var deviceUUID = miningPair.Device.UUID;
                        var deviceID = miningPair.Device.ID;

                        var apiDevice = apiDevices.Find(apiDev => apiDev.id == deviceID);
                        if (apiDevice.Equals(default(IdPowerHash))) continue;                       
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (algorithmType, apiDevice.speed * (1 - devFee * 0.01)) });
                        perDevicePowerInfo.Add(deviceUUID, apiDevice.power);
                        totalPower += apiDevice.power;                       

                        perDeviceAcceptedShareInfo.Add(deviceUUID, (apiDevice.accepted, apiDevice.lastAccepted));
                        perDeviceRejectedShareInfo.Add(deviceUUID, (apiDevice.rejected, apiDevice.lastRejected));
                    }
                }
                catch(Exception e)
                {
                    Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                }
            }
            var ads = new ApiDataShare(new ApiData());
            ads.PowerUsageTotal = totalPower;
            ads.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ads.PowerUsagePerDevice = perDevicePowerInfo;
            ads.ApiResponse = summaryApiResult;

            ads.AcceptedShareInfoPerDevice = perDeviceAcceptedShareInfo;
            ads.RejectedShareInfoPerDevice = perDeviceRejectedShareInfo;

            return ads;
        }

    }
}
