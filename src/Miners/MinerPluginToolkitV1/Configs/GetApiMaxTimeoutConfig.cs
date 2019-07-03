using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Configs
{
    [Serializable]
    public class GetApiMaxTimeoutConfig
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonProperty("general_timeout")]
        public TimeSpan GeneralTimeout { get; set; } = new TimeSpan();

        [JsonProperty("timeout_per_algorithm")]
        public Dictionary<string, TimeSpan> TimeoutPerAlgorithm { get; set; } = new Dictionary<string, TimeSpan>();

        [JsonProperty("timeout_per_device_type")]
        public Dictionary<DeviceType, TimeSpan> TimeoutPerDeviceType { get; set; } = new Dictionary<DeviceType, TimeSpan>();
    }
}
