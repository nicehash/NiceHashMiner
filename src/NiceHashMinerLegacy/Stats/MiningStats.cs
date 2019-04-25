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
        // TODO create base class from DeviceMiningStats and MinerMiningStats
        // We transform ApiData PerDevice info
        public class DeviceMiningStats
        {
            public string DeviceUUID { get; set; } = "";
            public List<(AlgorithmType type, double speed)> Speeds { get; set; } = new List<(AlgorithmType type, double speed)>();
            public List<(AlgorithmType type, double rate)> Rates { get; set; } = new List<(AlgorithmType type, double rate)>();

            // sources for PowerUsages
            public double PowerUsageAPI { get; set; } = 0d;
            public double PowerUsageDeviceReading { get; set; } = 0d;
            public double PowerUsageAlgorithmSetting { get; set; } = 0d;

            // add methods
            public double TotalPayingRate()
            {
                return Rates.Select(rateInfo => rateInfo.rate).Sum();
            }

            public double GetPowerUsage()
            {
                if (PowerUsageAPI > 0)
                {
                    return PowerUsageAPI;
                }
                if (PowerUsageDeviceReading > 0)
                {
                    return PowerUsageDeviceReading;
                }
                if (PowerUsageAlgorithmSetting > 0)
                {
                    return PowerUsageAlgorithmSetting;
                }
                return 0d;
            }

            public void Clear()
            {
                Speeds.Clear();
                Rates.Clear();
                PowerUsageAPI = 0;
                PowerUsageDeviceReading = 0;
                PowerUsageAlgorithmSetting = 0;
            }
        }

        // We transform ApiData Total info
        public class MinerMiningStats
        {
            public string GroupKey { get; set; } = "";

            public HashSet<string> DeviceUUIDs = new HashSet<string>();

            public List<(AlgorithmType type, double speed)> Speeds { get; set; } = new List<(AlgorithmType type, double speed)>();

            public List<(AlgorithmType type, double rate)> Rates { get; set; } = new List<(AlgorithmType type, double rate)>();

            // sources for PowerUsages
            public double PowerUsageAPI { get; set; } = 0d;
            public double PowerUsageDeviceReading { get; set; } = 0d;
            public double PowerUsageAlgorithmSetting { get; set; } = 0d;

            public double TotalPayingRate()
            {
                return Rates.Select(rateInfo => rateInfo.rate).Sum();
            }

            public double GetPowerUsage()
            {
                if (PowerUsageAPI > 0)
                {
                    return PowerUsageAPI;
                }
                if (PowerUsageDeviceReading > 0)
                {
                    return PowerUsageDeviceReading;
                }
                if (PowerUsageAlgorithmSetting > 0)
                {
                    return PowerUsageAlgorithmSetting;
                }
                return 0d;
            }

            public double TotalPayingRateDeductPowerCost(double kwhPriceInBtc)
            {
                var totalRate = TotalPayingRate();
                var powerUsage = GetPowerUsage();

                // Deduct power costs
                totalRate -= kwhPriceInBtc * powerUsage * 24 / 1000;

                return totalRate;
            }

            public void Clear()
            {
                DeviceUUIDs.Clear();
                Speeds.Clear();
                Rates.Clear();
                PowerUsageAPI = 0;
                //PowerUsageDeviceReading = 0;
                //PowerUsageAlgorithmSetting = 0;
            }

            public MinerMiningStats DeepCopy()
            {
                var copy = new MinerMiningStats
                {
                    GroupKey = this.GroupKey,
                    DeviceUUIDs = new HashSet<string>(this.DeviceUUIDs),
                    Speeds = this.Speeds.ToList(),
                    Rates = this.Rates.ToList(),
                    PowerUsageAPI = this.PowerUsageAPI
                };

                return copy;
            }
        }

        private static object _lock = new object();

        // key is joined device MinerUUID-UUIDs, sorted uuids
        private static Dictionary<string, ApiData> _apiDataGroups = new Dictionary<string, ApiData>();

        // key is joined device MinerUUID-UUIDs, sorted uuids
        private static Dictionary<string, MinerMiningStats> _minerMiningStats = new Dictionary<string, MinerMiningStats>();

        // key is device UUID 
        private static Dictionary<string, DeviceMiningStats> _deviceMiningStats = new Dictionary<string, DeviceMiningStats>();

        public static void UpdateGroup(ApiData apiData, string minerUUID)
        {

            if (apiData == null) return;
            if (apiData.AlgorithmSpeedsPerDevice == null || apiData.AlgorithmSpeedsPerDevice.Count == 0) return;
            if (apiData.PowerUsagePerDevice == null || apiData.PowerUsagePerDevice.Count == 0) return;
            if (apiData.AlgorithmSpeedsTotal == null || apiData.AlgorithmSpeedsTotal.Count == 0) return;

            var sortedDeviceUUIDs = apiData.AlgorithmSpeedsPerDevice.Select(speedInfo => speedInfo.Key).OrderBy(uuid => uuid).ToList();
            var uuidsKeys = string.Join(",", sortedDeviceUUIDs);
            

            var groupKey = $"{minerUUID}-{uuidsKeys}";
            
            // update groups
            lock (_lock)
            {
                // check what keys to remove
                var removeKeys = _apiDataGroups.Keys.Where(checkKey => {
                    var minerUUIDDiffers = checkKey.StartsWith(minerUUID) == false;
                    var deviceInKey = sortedDeviceUUIDs.Any(uuid => checkKey.Contains(uuid));
                    return minerUUIDDiffers && deviceInKey;
                }).ToArray();
                foreach (var removeKey in removeKeys)
                {
                    _apiDataGroups.Remove(removeKey);
                    _minerMiningStats.Remove(removeKey);
                }
                // add / update data
                _apiDataGroups[groupKey] = apiData;

                var payingRates = NHSmaData.CurrentProfitsSnapshot();
                // update stats
                UpdateMinerMiningStats(apiData, minerUUID, groupKey, payingRates);
                // update device stats
                foreach (var deviceUuid in sortedDeviceUUIDs)
                {
                    UpdateDeviceMiningStats(apiData, minerUUID, deviceUuid, payingRates);
                }

                // TODO notify change
            }
        }

        private static void UpdateMinerMiningStats(ApiData apiData, string minerUUID, string groupKey, Dictionary<AlgorithmType, double> payingRates)
        {
            MinerMiningStats stat;
            if (_minerMiningStats.TryGetValue(groupKey, out stat) == false)
            {
                // create if it doesn't exist
                stat = new MinerMiningStats { GroupKey = groupKey };
                _minerMiningStats[groupKey] = stat;
            }
            var deviceUUIDs = apiData.AlgorithmSpeedsPerDevice.Select(speedInfo => speedInfo.Key).ToArray();
            stat.Clear();

            // add device UUIDs
            foreach (var deviceUuid in deviceUUIDs) stat.DeviceUUIDs.Add(deviceUuid);

            // update stat
            stat.PowerUsageAPI = (double)apiData.PowerUsageTotal / 1000d;
            foreach (var speedInfo in apiData.AlgorithmSpeedsTotal)
            {
                stat.Speeds.Add((speedInfo.AlgorithmType, speedInfo.Speed));
                if (payingRates.TryGetValue(speedInfo.AlgorithmType, out var paying) == false) continue;
                var payingRate = paying * speedInfo.Speed * 0.000000001;
                stat.Rates.Add((speedInfo.AlgorithmType, payingRate));
            }
            var relevantDevices = AvailableDevices.Devices.Where(dev => deviceUUIDs.Contains(dev.Uuid)).ToArray();
            double powerUsageFromDevice = relevantDevices.Select(dev => dev.PowerUsage).Sum();
            double powerUsageFromAlgorithmSettings = relevantDevices
                .Select(dev => dev.GetAlgorithm(minerUUID, apiData.AlgorithmSpeedsPerDevice[dev.Uuid].Select(info => info.AlgorithmType).ToArray()))
                .Select(algo => algo == null ? 0d : algo.PowerUsage)
                .Sum();
            stat.PowerUsageDeviceReading = powerUsageFromDevice;
            stat.PowerUsageAlgorithmSetting = powerUsageFromAlgorithmSettings;
        }

        private static void UpdateDeviceMiningStats(ApiData apiData, string minerUUID, string deviceUuid, Dictionary<AlgorithmType, double> payingRates)
        {
            DeviceMiningStats stat;
            if (_deviceMiningStats.TryGetValue(deviceUuid, out stat) == false)
            {
                // create if it doesn't exist
                stat = new DeviceMiningStats { DeviceUUID = deviceUuid };
                _deviceMiningStats[deviceUuid] = stat;
            }
            stat.Clear();

            // update stat
            var deviceSpeedsInfo = apiData.AlgorithmSpeedsPerDevice
                .Where(pair => pair.Key == deviceUuid)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            if (deviceSpeedsInfo != null)
            {
                foreach (var speedInfo in deviceSpeedsInfo)
                {
                    stat.Speeds.Add((speedInfo.AlgorithmType, speedInfo.Speed));
                    if (payingRates.TryGetValue(speedInfo.AlgorithmType, out var paying) == false) continue;
                    var payingRate = paying * speedInfo.Speed * 0.000000001;
                    stat.Rates.Add((speedInfo.AlgorithmType, payingRate));
                }
            }

            var devicePowerUsageApi = (double)apiData.PowerUsagePerDevice
                .Where(pair => pair.Key == deviceUuid)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            stat.PowerUsageAPI = devicePowerUsageApi / 1000d;

            // TODO Globals
            var dev = AvailableDevices.GetDeviceWithUuid(deviceUuid);
            if (dev != null)
            {
                stat.PowerUsageDeviceReading = dev.PowerUsage;
                var algo = dev.GetAlgorithm(minerUUID, apiData.AlgorithmSpeedsPerDevice[dev.Uuid].Select(info => info.AlgorithmType).ToArray());
                stat.PowerUsageAlgorithmSetting = algo == null ? 0d : algo.PowerUsage;
            }
        }

        public static void ClearApiDataGroups()
        {
            _apiDataGroups.Clear();
            _minerMiningStats.Clear();
            _deviceMiningStats.Clear();
            // TODO notify change
        }

        public static List<(AlgorithmType type, double speed)> GetSpeedForDevice(string deviceUuid)
        {
            var ret = new List<(AlgorithmType type, double speed)>();
            lock (_lock)
            {
                if(_deviceMiningStats.TryGetValue(deviceUuid, out var stat))
                {
                    foreach (var speedInfo in stat.Speeds)
                    {
                        ret.Add(speedInfo);
                    }
                }
            }
            return ret;
        }

        //// TODO this should get calculate speeds paying rates and deduct power cost based on measured device usage or on measured algorithm usage
        //public static double GetTotalRate(Dictionary<AlgorithmType, double> payingRates)
        //{
        //    double totalRate = 0;
        //    var KwhPriceInBtc = ExchangeRateApi.GetKwhPriceInBtc();
        //    lock (_lock)
        //    {
        //        foreach (var ad in _apiDataGroups)
        //        {
        //            var minerUUID = ad.Key.Split('-')[0];
        //            var apiData = ad.Value;
        //            var deviceUUIDs = apiData.AlgorithmSpeedsPerDevice.Keys.ToArray();
        //            var currentPayingRate = 0d;
        //            foreach (var algorithmTotal in apiData.AlgorithmSpeedsTotal)
        //            {
        //                if (payingRates.TryGetValue(algorithmTotal.AlgorithmType, out var paying) == false) continue;
        //                currentPayingRate += paying * algorithmTotal.Speed * 0.000000001;
        //            }
        //            // power usage first try to get power usage from the api data, then devices and finally from the settings
        //            double powerUsageFromApiData = apiData.PowerUsageTotal > 0 ? (double)apiData.PowerUsageTotal : 0d;
        //            var relevantDevices = AvailableDevices.Devices.Where(dev => deviceUUIDs.Contains(dev.Uuid)).ToArray();
        //            double powerUsageFromDevice = relevantDevices.Select(dev => dev.PowerUsage).Sum();
        //            double powerUsageFromAlgorithmSettings = relevantDevices
        //                .Select(dev => dev.GetAlgorithm(minerUUID, apiData.AlgorithmSpeedsPerDevice[dev.Uuid].Select(info => info.AlgorithmType).ToArray()))
        //                .Select(algo => algo == null ? 0d : algo.PowerUsage)
        //                .Sum();

        //            var powerUsage = 0d;
        //            if (powerUsageFromApiData > 0)
        //            {
        //                // API data returns W instead of kW
        //                powerUsage = powerUsageFromApiData / 1000; 
        //            }
        //            else if (powerUsageFromDevice > 0)
        //            {
        //                powerUsage = powerUsageFromDevice;
        //            }
        //            else if (powerUsageFromAlgorithmSettings > 0)
        //            {
        //                powerUsage = powerUsageFromAlgorithmSettings;
        //            }

        //            // Deduct power costs
        //            currentPayingRate -= KwhPriceInBtc * powerUsage * 24 / 1000;
        //            // add to total
        //            totalRate += currentPayingRate;
        //        }
        //    }

        //    return totalRate;
        //}

        //// TODO REMOVE
        //public static double GetTotalRate()
        //{
        //    double totalRate = 0;
        //    var KwhPriceInBtc = ExchangeRateApi.GetKwhPriceInBtc();
        //    lock (_lock)
        //    {
        //        foreach (var deviceStatPair in _deviceMiningStats)
        //        {
        //            var key = deviceStatPair.Key;
        //            var deviceStat = deviceStatPair.Value;

        //            var powerUsage = deviceStat.GetPowerUsage();
        //            var currentPayingRate = deviceStat.TotalPayingRate();
        //            // Deduct power costs
        //            currentPayingRate -= KwhPriceInBtc * powerUsage * 24 / 1000;
        //            // add to total
        //            totalRate += currentPayingRate;
        //        }
        //    }

        //    return totalRate;
        //}

        // TODO this one doesn't have deducted
        // For Production
        public static List<MinerMiningStats> GetMinersMiningStats()
        {
            var ret = new List<MinerMiningStats>();
            lock (_lock)
            {
                foreach (var minerStatPair in _minerMiningStats)
                {
                    var key = minerStatPair.Key;
                    var minerStat = minerStatPair.Value;

                    ret.Add(minerStat.DeepCopy());
                }
            }
            return ret;
        }
    }
}
