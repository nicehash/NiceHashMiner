using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinerPluginToolkitV1.ExtraLaunchParameters
{
    [Serializable]
    public class MinerOptionsPackage
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("general_options")]
        public List<MinerOption> GeneralOptions { get; set; }

        [JsonProperty("temperature_options")]
        public List<MinerOption> TemperatureOptions { get; set; }
    }
}
