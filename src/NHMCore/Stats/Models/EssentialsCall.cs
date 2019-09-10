using Newtonsoft.Json;
using System.Collections.Generic;

namespace NHMCore.Stats.Models
{
    public class EssentialsCall
    {
        [JsonProperty("d")]
        public List<object> Devices;

        [JsonProperty("l")]
        public List<List<string>> Versions;
    }
}
