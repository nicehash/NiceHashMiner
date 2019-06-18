using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Common.Enums
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
