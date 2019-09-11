using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Interfaces.StateSetters
{
    public interface IEnabledDeviceStateSetter : IStateSetter
    {
        event EventHandler<(string uuid, bool enabled)> SetDeviceEnabledState;
    }
}
