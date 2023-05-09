using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public interface ISetFanSpeedPercentage
    {
        bool SetFanSpeedPercentage(int percentage);
    }
}
