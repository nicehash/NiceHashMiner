using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    public interface IDriverIsMinimumRequired
    {
        (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRequired(BaseDevice device);
    }
}
