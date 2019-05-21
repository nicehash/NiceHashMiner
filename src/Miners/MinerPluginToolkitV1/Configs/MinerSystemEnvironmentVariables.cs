using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinerPluginToolkitV1.Configs
{
    /// <summary>
    /// MinerSystemEnvironmentVariables class is used to define 
    /// </summary>
    [Serializable]
    public class MinerSystemEnvironmentVariables
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("default_system_environment_variables")]
        public Dictionary<string, string> DefaultSystemEnvironmentVariables { get; set; } = null;
        
        [JsonProperty("custom_system_environment_variables")]
        public Dictionary<string, Dictionary<string, string>> CustomSystemEnvironmentVariables { get; set; } = null;
    }
}
