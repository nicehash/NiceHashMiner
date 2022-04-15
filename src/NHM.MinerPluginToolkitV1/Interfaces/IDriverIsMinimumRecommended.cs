using NHM.Common.Device;
using NHM.Common.Enums;
using System;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    public interface IDriverIsMinimumRecommended
    {
        (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRecommended(BaseDevice device);
    }
}
