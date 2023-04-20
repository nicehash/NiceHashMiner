using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Common.Enums
{
    public enum DeviceDynamicProperties
    {
        NONE,
        Load,
        MemoryControllerLoad,
        Temperature,
        FanSpeedPercentage,
        PowerUsage,
        VramTemp,
        HotspotTemp,
        CoreClock,
        MemClock,
        TDP,
        TDPWatts,
        CoreVoltage,
        CoreClockDelta,
        MemClockDelta,
        FanSpeedRPM
    }
}
