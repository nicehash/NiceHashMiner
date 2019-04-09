using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmrStak.Configs
{
    class CachedAmdSettings
    {
        public List<string> DeviceUUIDs { get; set; }
        public AmdConfig CachedConfig { get; set; }
    }
}
