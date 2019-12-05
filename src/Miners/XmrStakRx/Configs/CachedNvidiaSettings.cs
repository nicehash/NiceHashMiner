using System.Collections.Generic;

namespace XmrStakRx.Configs
{
    class CachedNvidiaSettings
    {
        public List<string> DeviceUUIDs { get; set; }
        public NvidiaConfig CachedConfig { get; set; }
    }
}
