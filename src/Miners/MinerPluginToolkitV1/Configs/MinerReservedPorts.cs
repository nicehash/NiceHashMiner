using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.Configs
{
    [Serializable]
    public class MinerReservedPorts
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("algorithm_reserved_ports")]
        public Dictionary<string, List<int>> AlgorithmReservedPorts { get; set; } = null;
    }
}
