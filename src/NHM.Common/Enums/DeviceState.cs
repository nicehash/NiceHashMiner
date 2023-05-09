
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
        Gaming,
        Testing
#endif
        // TODO Extra states, NotProfitable
    }
}
