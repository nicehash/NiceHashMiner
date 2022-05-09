
namespace NHMCore.Nhmws
{
    enum ErrorCode : int
    {
        NoError = 0,
        DisabledDevice = -7,
        InvalidDevice = -6,
        InvalidWorker = -5,
        InvalidUsername = -4, // BTC
        UnableToHandleRpc = -3,
        NonExistentDevice = -2,
        RedundantRpc = -1,
        InternalNhmError = 1,
    }
}
