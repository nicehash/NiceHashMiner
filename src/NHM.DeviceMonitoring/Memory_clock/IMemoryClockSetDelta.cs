using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.Memory_clock
{
    public interface IMemoryClockSetDelta
    {
        bool SetMemoryClockDelta(int memoryClock);
        bool ResetMemoryClockDelta();
    }
}
