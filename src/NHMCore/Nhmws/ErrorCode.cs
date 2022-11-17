
namespace NHMCore.Nhmws
{
    public enum ErrorCode : int
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

        ActionNotFound = -100,
        ErrNoDeviceRunning = -101,
        TargetDeviceNotFound = -102,
        TestApplyPartialPartial = -103,
        TestApplyTotalFail = -104,


    }
}
