using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.TDP
{
    public interface ITDPLimits
    {
        (bool ok, int min, int max, int def) GetTDPLimits();
    }
}
