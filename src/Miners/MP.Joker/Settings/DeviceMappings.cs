using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MP.Joker.Settings
{
    class DeviceMappings
    {
        [JsonProperty("device_uuid")]
        public string DeviceUUID { get; set; } = null;
        
        [JsonProperty("pcie")]
        public int Pcie { get; set; } = -1;
        
        [JsonProperty("miner_device_id")]
        public int MinerDeviceId { get; set; } = -1;
    }
}
