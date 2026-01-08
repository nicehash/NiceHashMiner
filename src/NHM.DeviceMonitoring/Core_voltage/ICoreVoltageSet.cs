using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.Core_voltage
{
    public interface ICoreVoltageSet
    {
        bool SetCoreVoltage(int coreVoltage);
        bool ResetCoreVoltage();
    }
}
