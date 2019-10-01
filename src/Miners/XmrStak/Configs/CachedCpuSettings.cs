using System.Collections.Generic;

namespace XmrStak.Configs
{
    class CachedCpuSettings
    {
        public List<string> DeviceUUIDs { get; set; }
        public CpuConfig CachedConfig { get; set; }
    }
}
