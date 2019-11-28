using System.Linq;

namespace NHMCore.Mining.MiningStats
{
    // We transform ApiData PerDevice info
    public class DeviceMiningStats : BaseStats
    {
        public string DeviceUUID { get; set; } = "";

        public DeviceMiningStats DeepCopy()
        {
            var copy = new DeviceMiningStats
            {
                // BaseStats
                MinerName = this.MinerName,
                GroupKey = this.GroupKey,
                Speeds = this.Speeds.ToList(),
                Rates = this.Rates.ToList(),
                PowerUsageAPI = this.PowerUsageAPI,
                PowerUsageDeviceReading = this.PowerUsageDeviceReading,
                PowerUsageAlgorithmSetting = this.PowerUsageAlgorithmSetting,
                // DeviceMiningStats
                DeviceUUID = this.DeviceUUID
            };

            return copy;
        }
    }
}
