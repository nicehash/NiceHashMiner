using NHM.Common.Device;
using NHM.Common.Enums;
using System;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    public interface IDriverIsMinimumRequired
    {
        (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRequired(BaseDevice device);
    }
}
