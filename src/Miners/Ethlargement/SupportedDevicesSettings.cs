using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethlargement
{
    public class SupportedDevicesSettings
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("supported_device_names")]
        public List<string> SupportedDeviceNames { get; set; } = null;
    }
}
