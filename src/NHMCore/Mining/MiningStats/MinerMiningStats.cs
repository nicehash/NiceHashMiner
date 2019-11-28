using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Mining.MiningStats
{
    // We transform ApiData Total info
    public class MinerMiningStats : BaseStats
    {
        public HashSet<string> DeviceUUIDs = new HashSet<string>();

        public MinerMiningStats DeepCopy()
        {
            var copy = new MinerMiningStats
            {
                // BaseStats
                MinerName = this.MinerName,
                GroupKey = this.GroupKey,
                Speeds = this.Speeds.ToList(),
                Rates = this.Rates.ToList(),
                PowerUsageAPI = this.PowerUsageAPI,
                PowerUsageDeviceReading = this.PowerUsageDeviceReading,
                PowerUsageAlgorithmSetting = this.PowerUsageAlgorithmSetting,
                // MinerMiningStats
                DeviceUUIDs = new HashSet<string>(this.DeviceUUIDs)
            };

            return copy;
        }
    }
}
