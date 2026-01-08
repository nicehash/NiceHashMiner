
using System;
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
        Disabled,
        [Obsolete("UNUSED status", true)]
        Gaming,
#if NHMWS4
        Testing,
#endif
    }
}
