
using System;
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
#if NHMWS4
        [Obsolete("UNUSED status", true)]
        Gaming,
        Testing
#endif
        // TODO Extra states, NotProfitable
    }
}
