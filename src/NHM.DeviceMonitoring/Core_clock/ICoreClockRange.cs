using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.Core_clock
{
    public interface ICoreClockRange
    {
        (bool ok, int min, int max, int def) CoreClockRange { get; }
    }
}
