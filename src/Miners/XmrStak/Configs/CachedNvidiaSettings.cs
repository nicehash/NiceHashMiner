using System.Collections.Generic;

namespace XmrStak.Configs
{
    class CachedNvidiaSettings
    {
        public List<string> DeviceUUIDs { get; set; }
        public NvidiaConfig CachedConfig { get; set; }
    }
}
