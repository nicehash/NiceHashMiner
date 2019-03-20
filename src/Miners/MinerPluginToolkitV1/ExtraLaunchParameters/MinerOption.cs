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
        /// <summary>
        /// Type represents which MinerOptionType is being used for parameter.
        /// </summary>
        /// <example>MinerOptionType.OptionWithMultiParameters</example>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")]
        public MinerOptionType Type { get; set; }

        /// <summary>
        /// ID represents internal id for parameter.
        /// </summary>
        /// <example>"ttminer_intensity"</example>
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// ShortName represents short name for parameter launch.
        /// </summary>
        /// <example>"-i"</example>
        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        /// <summary>
        /// LongName represents long name for parameter launch if exists.
        /// </summary>
        /// <example>"--intensity"</example>
        [JsonProperty("long_name")]
        public string LongName { get; set; }

        /// <summary>
        /// DefaultValue represents default value for parameter.
        /// </summary>
        /// <example>"-1"</example>
        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Delimeter represents character which divides parameters (if multiple).
        /// </summary>
        /// <example>","</example>
        [JsonProperty("delimiter")]
        public string Delimiter { get; set; }
    }
}
