using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public interface ITDPLimits
    {
        (bool ok, uint min, uint max, uint def) GetTDPLimits();
    }
}
