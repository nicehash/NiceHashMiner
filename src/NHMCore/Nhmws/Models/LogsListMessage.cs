using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Nhmws.Models
{
    class LogsListMessage
    {
        public string method => "executed";
        [JsonProperty("params")]
        public List<object> Params = new List<object>();
        public List<string> data = new List<string>();

        public LogsListMessage(int message_id, int error_code, string error_message, List<string> logsData = null)
        {
            Params.Add(message_id);
            Params.Add(error_code);
            if (error_message != null)
            {
                Params.Add(error_message);
            }
            if(logsData != null)
            {
                data = logsData;
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    } 
}
