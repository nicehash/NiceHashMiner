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
                    if (e.Message != "An item with the same key has already been added.")
                    {
                        Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }
            }
            var threadsApiResult = await GetApiDataThreads(port, logGroup);
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
            var perDevicePowerInfo = new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(threadsApiResult))
            {
                var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var gpu in gpus)
                {
                    // try inside loop so an error in one value will not prevent the others from being reported
                    try
                    {
                        // RemoveEmptyEntries will not remove NUL chars as they are technically not empty, and result is NUL-terminated
                        if (gpu == "\0") continue;

                        var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var gpuData = new IdPowerHash();

                        var idFound = false;

                        foreach (var optvalPairs in gpuOptvalPairs)
                        {
                            var optval = optvalPairs.Split(new char[] { '=' },
                                StringSplitOptions.RemoveEmptyEntries);
                            if (optval.Length != 2) continue;
                            if (optval[0] == "GPU")
                            {
                                gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                idFound = true;
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

                        // gpuData.id will have its default value of 0 if for some reason the data did not 
                        // contain a valid GPU entry. So, skip in this case (otherwise it will set bad data for first GPU)
                        if (!idFound) continue;

                        var device = miningPairs
                            .Where(kvp => kvp.Device.ID == gpuData.id)
                            .Select(kvp => kvp.Device)
                            .FirstOrDefault();

                        if (device == null) continue;

                        perDeviceSpeedInfo[device.UUID] =
                            new List<AlgorithmTypeSpeedPair>
                            {
                                new AlgorithmTypeSpeedPair(algorithmType, gpuData.speed * (1 - devFee * 0.01))
                            };
                        perDevicePowerInfo[device.UUID] = gpuData.power;
                        totalPower += gpuData.power;
                    }
                    catch (Exception e)
                    {
                        // Catch and proceed to try next entry
                        Logger.Error(logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }
            }
            var ad = new ApiData();
            ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(algorithmType, totalSpeed * (1 - devFee * 0.01)) };
            ad.PowerUsageTotal = totalPower;
            ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ad.PowerUsagePerDevice = perDevicePowerInfo;

            return ad;
        }

    }
}
