using System.Collections.Generic;

namespace XmrStakRx.Configs
{
    class CachedAmdSettings
    {
        public List<string> DeviceUUIDs { get; set; }
        public AmdConfig CachedConfig { get; set; }
    }
}
