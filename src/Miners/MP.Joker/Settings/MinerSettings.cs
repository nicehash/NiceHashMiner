using Newtonsoft.Json;
using NHM.MinerPluginToolkitV1.Configs;
using static MP.Joker.PluginEngines;

namespace MP.Joker.Settings
{
    internal class MinerSettings : MinerCommandLineSettings
    {
        [JsonProperty("init")]
        public bool Init { get; set; } = false;

        public PluginEngine PluginEngine { get; set; } = PluginEngine.Unknown;

        // TODO make this obsolete
        //[JsonProperty("conection_type")]
        //public NhmConectionType NhmConectionType { get; set; } = NhmConectionType.NONE;

        //[JsonProperty("devices_separator")]
        //public string DevicesSeparator { get; set; } = ",";

        // TODO make this obsolete
        //[JsonProperty("default_command_line")]
        //public string DefaultCommandLine { get; set; } = $"--user {USERNAME_TEMPLATE} --pool {POOL_URL_TEMPLATE}:{POOL_PORT_TEMPLATE} --algo {ALGORITHM_TEMPLATE} --apiport {API_PORT_TEMPLATE} --devices {DEVICES_TEMPLATE} {EXTRA_LAUNCH_PARAMETERS_TEMPLATE}";

        //[JsonProperty("algorithm_command_line")]
        //public Dictionary<AlgorithmType, string> AlgorithmCommandLine { get; set; } = new Dictionary<AlgorithmType, string>{

        //    {AlgorithmType.DaggerHashimoto, $"--user {USERNAME_TEMPLATE} --pool {POOL_URL_TEMPLATE}:{POOL_PORT_TEMPLATE} --algo {ALGORITHM_TEMPLATE} --apiport {API_PORT_TEMPLATE} --devices {DEVICES_TEMPLATE} {EXTRA_LAUNCH_PARAMETERS_TEMPLATE}"},
        //};
    }
}
