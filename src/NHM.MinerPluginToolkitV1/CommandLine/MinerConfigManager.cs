using Newtonsoft.Json;
using NHM.Common;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.CommandLine
{
    public static class MinerConfigManager
    {
        [Serializable]
        public class MinerConfig
        {
            [JsonProperty("miner_name", Order = 1)]
            public string MinerName { get; set; }

            [JsonProperty("miner_uuid", Order = 1)]
            public string MinerUUID { get; set; }

            [JsonProperty("miner_command", Order = 2)]
            public List<List<string>> MinerCommands = new();

            [JsonProperty("algos", Order = 3)]
            public List<Algo> Algorithms = new();
        }

        [Serializable]
        public class Algo
        {
            [JsonProperty("algo", Order = 1)]
            public string AlgorithmName { get; set; }

            [JsonProperty("algorithm_command", Order = 2)]
            public List<List<string>> AlgoCommands = new();

            [JsonProperty("devices", Order = 3)]
            public Dictionary<string, List<List<string>>> Devices = new();
        }

        public static void WriteConfig(MinerConfig minerConfig)
        {
            var path = Paths.ConfigsPath(minerConfig.MinerName + "-" + minerConfig.MinerUUID);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(minerConfig, Formatting.Indented));
            }
            else
            {
                var data = ReadConfig(path);
                data.MinerCommands = minerConfig.MinerCommands;
                File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
        }

        public static MinerConfig ReadConfig(string path)
        {
            return JsonConvert.DeserializeObject<MinerConfig>(File.ReadAllText(path));
        }
    }
}
