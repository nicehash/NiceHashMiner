using Newtonsoft.Json;
using NHM.Common.Configs;
using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.Configs
{
    public class BenchmarkLineExceptions : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("benchmark_line_message_exceptions")]
        public Dictionary<string, string> BenchmarkLineMessageExceptions = new Dictionary<string, string> { };
    }
}
