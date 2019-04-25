using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinerPlugin;
using NiceHashMiner.Devices;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Stats
{
    // MiningStats are the new APIData rates groups...
    // keep this here for now
    public static class MiningStats
    {
        private static object _lock = new object();

        // key is joined device MinerUUID-UUIDs, sorted uuids
        private static Dictionary<string, ApiData> _apiDataGroups = new Dictionary<string, ApiData>();

        public static void UpdateGroup(ApiData data, string minerUUID)
        {

            if (data == null) return;
            if (data.AlgorithmSpeedsPerDevice == null || data.AlgorithmSpeedsPerDevice.Count == 0) return;
            var sortedUUIDs = data.AlgorithmSpeedsPerDevice.Select(speedInfo => speedInfo.Key).OrderBy(uuid => uuid).ToList();
            var uuidsKeys = string.Join(",", sortedUUIDs);
            

            var key = $"{minerUUID}-{uuidsKeys}";
            
            // update groups
            lock (_lock)
            {
                // check what keys to remove
                var removeKeys = _apiDataGroups.Keys.Where(checkKey => {
                    var minerUUIDDiffers = checkKey.StartsWith(minerUUID) == false;
                    var deviceInKey = sortedUUIDs.Any(uuid => checkKey.Contains(uuid));
                    return minerUUIDDiffers && deviceInKey;
                }).ToArray();
                foreach (var removeKey in removeKeys)
                {
                    _apiDataGroups.Remove(removeKey);
                }
                // add / update data
                _apiDataGroups[key] = data;
                // TODO notify change
            }
        }

        public static void ClearApiDataGroups()
        {
            _apiDataGroups.Clear();
        }

        public static List<(AlgorithmType type, double speed)> GetSpeedForDevice(string uuid)
        {
            // update groups
            lock (_lock)
            {
                ApiData data = _apiDataGroups
                    .Where(kvp => kvp.Key.Contains(uuid))
                    .Select(kvp => kvp.Value)
                    .FirstOrDefault();
                if (data == null) return null;
                var speeds = data.AlgorithmSpeedsPerDevice
                    .Where(speedInfo => speedInfo.Key == uuid)
                    .Select(speedInfo => speedInfo.Value)
                    .FirstOrDefault();
                if (speeds == null) return null;
                return speeds.Select(info => (type: info.AlgorithmType, speed: info.Speed)).ToList();
            }
            // no speed found
            return null;
        }

        // TODO this should get calculate speeds paying rates and deduct power cost based on measured device usage or on measured algorithm usage
        public static double GetTotalRate(Dictionary<AlgorithmType, double> payingRates)
        {
            double totalRate = 0;
            var KwhPriceInBtc = ExchangeRateApi.GetKwhPriceInBtc();
            lock (_lock)
            {
                foreach (var ad in _apiDataGroups)
                {
                    var minerUUID = ad.Key.Split('-')[0];
                    var apiData = ad.Value;
                    var deviceUUIDs = apiData.AlgorithmSpeedsPerDevice.Keys.ToArray();
                    var currentPayingRate = 0d;
                    foreach (var algorithmTotal in apiData.AlgorithmSpeedsTotal)
                    {
                        if (payingRates.TryGetValue(algorithmTotal.AlgorithmType, out var paying) == false) continue;
                        currentPayingRate += paying * algorithmTotal.Speed * 0.000000001;
                    }
                    // power usage first try to get power usage from the api data, then devices and finally from the settings
                    double powerUsageFromApiData = apiData.PowerUsageTotal > 0 ? (double)apiData.PowerUsageTotal : 0d;
                    var relevantDevices = AvailableDevices.Devices.Where(dev => deviceUUIDs.Contains(dev.Uuid)).ToArray();
                    double powerUsageFromDevice = relevantDevices.Select(dev => dev.PowerUsage).Sum();
                    double powerUsageFromAlgorithmSettings = relevantDevices
                        .Select(dev => dev.GetAlgorithm(minerUUID, apiData.AlgorithmSpeedsPerDevice[dev.Uuid].Select(info => info.AlgorithmType).ToArray()))
                        .Select(algo => algo == null ? 0d : algo.PowerUsage)
                        .Sum();

                    var powerUsage = 0d;
                    if (powerUsageFromApiData > 0)
                    {
                        // API data returns W instead of kW
                        powerUsage = powerUsageFromApiData / 1000; 
                    }
                    else if (powerUsageFromDevice > 0)
                    {
                        powerUsage = powerUsageFromDevice;
                    }
                    else if (powerUsageFromAlgorithmSettings > 0)
                    {
                        powerUsage = powerUsageFromAlgorithmSettings;
                    }

                    // Deduct power costs
                    currentPayingRate -= KwhPriceInBtc * powerUsage * 24 / 1000;
                    // add to total
                    totalRate += currentPayingRate;
                }
            }

            return totalRate;
        }
    }
}
