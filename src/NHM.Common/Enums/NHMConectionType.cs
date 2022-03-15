using System;

namespace NHM.Common.Enums
{
    public enum NhmConectionType
    {
        NONE,
        STRATUM_TCP,
        STRATUM_SSL,
        [Obsolete("UNUSED", false)]
        LOCKED, // inhouse miners that are locked on NH (our eqm)
        [Obsolete("UNUSED", true)]
        SSL
    }
}
