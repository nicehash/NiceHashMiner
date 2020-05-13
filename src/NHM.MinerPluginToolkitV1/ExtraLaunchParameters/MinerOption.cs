using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace NHM.MinerPluginToolkitV1.ExtraLaunchParameters
{
    /// <summary>
    /// MinerOption class gives user option to define extra launch parameter
    /// Definition of properties should not be changed!!!
    /// </summary>
    /// <example>
    /// <code>
    /// {
    ///     Type = MinerOptionType.OptionWithMultipleParameters,
    ///     ID = "gminer_templimit",
    ///     ShortName = "-t",
    ///     LongName = "--templimit",
    ///     DefaultValue = "90",
    ///     Delimiter = " "
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public class MinerOption
    {
        /// <summary>
        /// Type member indicates how to parse following values
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")]
        public MinerOptionType Type { get; set; }

        /// <summary>
        /// Id for extra launch parameter
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// ShortName represents short name for parameter launch.
        /// </summary>
        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        /// <summary>
        /// LongName represents long name for parameter launch if exists.
        /// </summary>
        [JsonProperty("long_name")]
        public string LongName { get; set; }

        /// <summary>
        /// DefaultValue represents default value for parameter.
        /// </summary>
        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Delimeter represents character which divides parameters (if multiple).
        /// </summary>
        [JsonProperty("delimiter")]
        public string Delimiter { get; set; }
    }
}
