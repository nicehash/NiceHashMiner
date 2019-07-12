using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NiceHashMiner.Stats.Models
{
    // PRODUCTION
#pragma warning disable 649, IDE1006
    class DeviceStatusMessage
    {
        public string method => "devices.status";
        public List<JArray> devices { get; set; }
    }
#pragma warning restore 649, IDE1006
}
