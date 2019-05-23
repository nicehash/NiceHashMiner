using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Configs
{
    /// <summary>
    /// MinerReservedPorts class is used to reserve specific ports for each algorithm
    /// </summary>
    [Serializable]
    public class MinerReservedPorts
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
