using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public interface IGetFanSpeedPercentage
    {
        (int status, int percentage) GetFanSpeedPercentage();
    }
}
