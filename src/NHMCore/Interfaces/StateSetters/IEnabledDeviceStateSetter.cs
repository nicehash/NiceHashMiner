using System;

namespace NHMCore.Interfaces.StateSetters
{
    public interface IEnabledDeviceStateSetter : IStateSetter
    {
        event EventHandler<(string uuid, bool enabled)> SetDeviceEnabledState;
    }
}
