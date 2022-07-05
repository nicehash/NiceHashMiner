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

            [JsonProperty("miner_uuid", Order = 2)]
            public string MinerUUID { get; set; }

            [JsonProperty("miner_command", Order = 3)]
            public List<List<string>> MinerCommands = new();

            [JsonProperty("algos", Order = 4)]
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
            public Dictionary<string, Device> Devices = new();
        }

        [Serializable]
        public class Device
        {
            [JsonProperty("device", Order = 1)]
            public string DeviceName { get; set; }
            [JsonProperty("commands", Order = 2)]
            public List<List<string>> Commands = new();
        }

        public static void WriteConfig(MinerConfig minerConfig, bool forceNew = false)
        {
            //test only path
            //var path = @"..\..\..\CommandLine\" + minerConfig.MinerName + "-" + minerConfig.MinerUUID + ".json";
            //program path
            var path = Paths.ConfigsPath(minerConfig.MinerName + "-" + minerConfig.MinerUUID + ".json");
            if (!File.Exists(path) || forceNew)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(minerConfig, Formatting.Indented));
            }
            else
            {
                var data = ReadConfig(minerConfig.MinerName, minerConfig.MinerUUID);
                data.MinerCommands = minerConfig.MinerCommands;

                foreach (var configAlgo in minerConfig.Algorithms)
                {
                    var algoExists = false;
                    foreach (var dataAlgo in data.Algorithms)
                    {
                        if (configAlgo.AlgorithmName != dataAlgo.AlgorithmName) continue;
                        dataAlgo.AlgoCommands = configAlgo.AlgoCommands;
                        algoExists = true;
                        foreach (var configDevice in configAlgo.Devices)
                        {
                            if (dataAlgo.Devices.ContainsKey(configDevice.Key))
                                dataAlgo.Devices[configDevice.Key] = configDevice.Value;
                            else
                                dataAlgo.Devices.Add(configDevice.Key, configDevice.Value);
                        }
                    }
                    if (!algoExists) data.Algorithms.Add(configAlgo);
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
        }

        public static MinerConfig ReadConfig(string minerName, string minerUUID)
        {
            var path = Paths.ConfigsPath(minerName + "-" + minerUUID + ".json");
            return JsonConvert.DeserializeObject<MinerConfig>(File.ReadAllText(path));
        }
        public static MinerConfig ReadConfig(string path)
        {
            return JsonConvert.DeserializeObject<MinerConfig>(File.ReadAllText(path));
        }
    }
}
