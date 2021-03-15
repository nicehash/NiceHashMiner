using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Mining.MiningStats
{
    public class BaseStats
    {
        public string MinerName { get; set; } = "";
        public string GroupKey { get; set; } = "";

        public List<(AlgorithmType type, double speed)> Speeds { get; set; } = new List<(AlgorithmType type, double speed)>();
        public List<(AlgorithmType type, double rate)> Rates { get; set; } = new List<(AlgorithmType type, double rate)>();

        // sources for PowerUsages
        public double PowerUsageAPI { get; set; } = 0d;
        public double PowerUsageDeviceReading { get; set; } = 0d;
        public double PowerUsageAlgorithmSetting { get; set; } = 0d;

        // add methods or Revenue in ApiData TESTNET
        public double TotalPayingRate()
        {
            return Rates.Select(rateInfo => rateInfo.rate).Sum();
        }

        // or Profit in ApiData TESTNET
        public double TotalPayingRateDeductPowerCost(double kwhPriceInBtc)
        {
            var totalRate = TotalPayingRate();
            var powerCost = PowerCost(kwhPriceInBtc);
            return totalRate - powerCost;
        }

        public double GetPowerUsage()
        {
            if (PowerUsageAPI > 0) return PowerUsageAPI;
            if (PowerUsageDeviceReading > 0) return PowerUsageDeviceReading;
            if (PowerUsageAlgorithmSetting > 0) return PowerUsageAlgorithmSetting;
            return 0d;
        }

        public double PowerCost(double kwhPriceInBtc)
        {
            var powerUsage = GetPowerUsage();
            return kwhPriceInBtc * powerUsage * 24 / 1000;
        }

        public void CopyFrom(BaseStats setFrom)
        {
            // BaseStats
            MinerName = setFrom.MinerName;
            GroupKey = setFrom.GroupKey;
            Speeds = setFrom.Speeds.ToList();
            Rates = setFrom.Rates.ToList();
            PowerUsageAPI = setFrom.PowerUsageAPI;
            PowerUsageDeviceReading = setFrom.PowerUsageDeviceReading;
            PowerUsageAlgorithmSetting = setFrom.PowerUsageAlgorithmSetting;
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
}
