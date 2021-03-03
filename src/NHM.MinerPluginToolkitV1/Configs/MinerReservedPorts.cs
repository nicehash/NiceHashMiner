using Newtonsoft.Json;
using NHM.Common.Configs;
using System;
using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.Configs
{
    /// <summary>
    /// MinerReservedPorts class is used to reserve specific ports for each algorithm
    /// </summary>
    /// <jsonSerializationExample>
    /// {
    ///     "use_user_settings": "true",
    ///     "algorithm_reserved_ports": {
    ///         "Beam": [4001, 4002],
    ///         "CuckooCycle": [4005, 4010]
    ///      }
    /// }
    /// </jsonSerializationExample>
    [Serializable]
    public class MinerReservedPorts : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        /// <summary>
        /// AlgorithmReservedPorts is a Dictionary with AlgorithmName for key and list of ports for value
        /// </summary>
        [JsonProperty("algorithm_reserved_ports")]
        public Dictionary<string, List<int>> AlgorithmReservedPorts { get; set; } = null;
    }
}
