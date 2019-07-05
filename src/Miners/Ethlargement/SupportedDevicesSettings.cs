using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ethlargement
{
    public class SupportedDevicesSettings : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("supported_device_names")]
        public List<string> SupportedDeviceNames { get; set; } = null;
    }
}
