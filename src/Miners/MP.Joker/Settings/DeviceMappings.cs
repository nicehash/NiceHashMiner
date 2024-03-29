﻿using Newtonsoft.Json;

namespace MP.Joker.Settings
{
    class DeviceMappings
    {
        [JsonProperty("compatible")]
        public bool Compatible { get; set; } = true;

        [JsonProperty("device_name")]
        public string DeviceName { get; set; } = null;

        [JsonProperty("device_uuid")]
        public string DeviceUUID { get; set; } = null;
        
        [JsonProperty("pcie")]
        public int Pcie { get; set; } = -1;
        
        [JsonProperty("miner_device_id")]
        public int MinerDeviceId { get; set; } = -1;
    }
}
