using Newtonsoft.Json;
using System.Collections.Generic;

namespace NHMCore.Nhmws.Models
{
    internal class ExecutedCall
    {
        public readonly string method = "executed";
        [JsonProperty("params")]
        public List<object> Params = new List<object>();

        public ExecutedCall(int id, int code, string message)
        {
            Params.Add(id);
            Params.Add(code);
            if (message != null)
            {
                Params.Add(message);
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
