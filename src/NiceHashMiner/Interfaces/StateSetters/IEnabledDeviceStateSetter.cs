using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces.StateSetters
{
    interface IEnabledDeviceStateSetter : IStateSetter
    {
        event EventHandler<(string uuid, bool enabled)> SetDeviceEnabledState;
    }
}
