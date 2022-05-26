using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NHMCore.Nhmws.V3
{
    internal class MinerStatusMessage : ISendMessage
    {
        [JsonProperty("method")]
        public string Method => "miner.status";
        [JsonProperty("params")]
        public List<JToken> Params { get; set; }
    }

    internal class LoginMessage : ISendMessage
    {
        [JsonProperty("method")]
        public string Method => "login";
        [JsonProperty("protocol")]
        public int Protocol => 3;
        [JsonProperty("version")]
        public string Version { get; set; } = "";
        [JsonProperty("btc")]
        public string Btc { get; set; } = "";
        [JsonProperty("rig")]
        public string Rig { get; set; } = "";
        [JsonProperty("worker")]
        public string Worker { get; set; } = "";
        [JsonProperty("group")]
        public string Group { get; set; } = "";
    }
}
