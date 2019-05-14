using MinerPlugin;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.CCMinerCommon
{
    public static class CCMinerAPIHelpers
    {
        public static async Task<string> GetApiDataBase(int port, string dataToSend, string logGroup)
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
                return null;
            }
        }

        public static Task<string> GetApiDataSummary(int port, string logGroup)
        {
            var dataToSend = ApiDataHelper.GetHttpRequestNhmAgentStrin("summary");
            return GetApiDataBase(port, dataToSend, logGroup);
        }

        public static Task<string> GetApiDataThreads(int port, string logGroup)
        {
            var dataToSend = ApiDataHelper.GetHttpRequestNhmAgentStrin("threads");
            return GetApiDataBase(port, dataToSend, logGroup);
        }

        private struct IdPowerHash
        {
            public int id;
            public int power;
            public double speed;
        }

        public static async Task<ApiData> GetMinerStatsDataAsync(int port, AlgorithmType algorithmType, IEnumerable<MiningPair> miningPairs, string logGroup, double DevFee)
        {
            var summaryApiResult = await GetApiDataSummary(port, logGroup);
            double totalSpeed = 0;
            int totalPower = 0;
            if (!string.IsNullOrEmpty(summaryApiResult))
            {
                // TODO return empty
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
                    if (e.Message != "An item with the same key has already been added.")
                    {
                        Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }
            }
            // TODO if have multiple GPUs call the threads as well, but maybe not as often since it might crash the miner
            //var threadsApiResult = await _httpClient.GetStringAsync($"{localhost}/threads");
            var threadsApiResult = await GetApiDataThreads(port, logGroup);
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
            var perDevicePowerInfo = new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(threadsApiResult))
            {
                // TODO return empty
                try
                {
                    var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
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
                        }
                        // TODO do stuff with it gpuData
                        var device = miningPairs.Where(kvp => kvp.Device.ID == gpuData.id).Select(kvp => kvp.Device).FirstOrDefault();
                        if (device != null)
                        {
                            perDeviceSpeedInfo.Add(device.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(algorithmType, gpuData.speed * (1 - DevFee * 0.01)) });
                            perDevicePowerInfo.Add(device.UUID, gpuData.power);
                            totalPower += gpuData.power;
                        }

                    }
                }
                catch(Exception e)
                {
                    if (e.Message != "An item with the same key has already been added.")
                    {
                        Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }
            }
            var ad = new ApiData();
            ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(algorithmType, totalSpeed * (1 - DevFee * 0.01)) };
            ad.PowerUsageTotal = totalPower;
            ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ad.PowerUsagePerDevice = perDevicePowerInfo;

            return ad;
        }

    }
}
