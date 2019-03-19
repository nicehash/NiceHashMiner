using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinerPluginToolkitV1.ExtraLaunchParameters
{
    [Serializable]
    public class MinerOption
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")]
        public MinerOptionType Type { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("long_name")]
        public string LongName { get; set; }

        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        [JsonProperty("delimiter")]
        public string Delimiter { get; set; }
    }
}
