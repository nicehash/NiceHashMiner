using System;
using System.Collections.Generic;
using System.Text;

namespace NHM.Common.Enums
{
    public enum RigStatus
    {
        Offline = 0,
        Stopped,
        Mining,
        Benchmarking,
        Error,
        Pending,
        Disabled
    }
}
