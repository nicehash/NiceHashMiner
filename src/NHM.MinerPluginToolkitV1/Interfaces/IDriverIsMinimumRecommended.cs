using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    public interface IDriverIsMinimumRecommended
    {
        (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRecommended(BaseDevice device);
    }
}
