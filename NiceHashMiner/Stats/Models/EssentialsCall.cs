using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NiceHashMiner.Stats.Models
{
    public class EssentialsCall
    {
        [JsonProperty("params")]
        public List<List<object>> Params;
    }
}
