using System.Collections.Generic;

namespace XmrStakRx.Configs
{
    class CachedCpuSettings
    {
        public List<string> DeviceUUIDs { get; set; }
        public CpuConfig CachedConfig { get; set; }
    }
}
