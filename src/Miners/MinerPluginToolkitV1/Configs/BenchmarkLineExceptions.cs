using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.Configs
{
    public class BenchmarkLineExceptions : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("benchmark_line_message_exceptions")]
        public Dictionary<string, string> BenchmarkLineMessageExceptions = new Dictionary<string, string> { };
    }
}
