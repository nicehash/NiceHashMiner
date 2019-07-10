using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
