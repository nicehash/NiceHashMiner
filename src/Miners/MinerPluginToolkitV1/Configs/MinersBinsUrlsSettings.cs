using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.Configs
{
    public class MinersBinsUrlsSettings : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("bins_urls")]
        public List<string> Urls { get; set; } = null;
    }
}
