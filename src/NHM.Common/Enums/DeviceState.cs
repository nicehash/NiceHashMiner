using System;
using System.Collections.Generic;
using System.Text;

namespace NHM.Common.Enums
{
    public enum DeviceState
    {
        Stopped,
        Mining,
        Benchmarking,
        Error,
        Pending,
        Disabled,
        // TODO Extra states, NotProfitable
    }
}
