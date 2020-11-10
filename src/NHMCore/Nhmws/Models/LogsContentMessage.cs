using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Nhmws.Models
{
    class LogsContentMessage
    {
        public string method => "executed";
        [JsonProperty("params")]
        public List<object> Params = new List<object>();
        public string content = "";

        public LogsContentMessage(int message_id, int error_code, string error_message, string logContent = "")
        {
            Params.Add(message_id);
            Params.Add(error_code);
            if (error_message != null)
            {
                Params.Add(error_message);
            }
            if(!string.IsNullOrEmpty(logContent))
            {
                content = logContent;
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    } 
}
