using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.Memory_clock
{
    public interface IMemoryClockRange
    {
        (int min, int max) GetMemoryClockRange { get; }
        (int min, int max) GetMemoryClockDeltaRange { get; }
    }
}
