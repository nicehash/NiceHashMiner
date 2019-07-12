using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NiceHashMiner.Stats.Models
{
    // TESTNET || TESTNETDEV || PRODUCTION_NEW
#pragma warning disable 649, IDE1006
    class MinerStatusMessage
    {
        public string method => "miner.status";
        [JsonProperty("params")]
        public List<JToken> param { get; set; }
    }
#pragma warning restore 649, IDE1006
}
