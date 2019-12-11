
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
        TerminalError = 50,
        MultipleMinerErrors = 100,
        InternalRPCTimeout = 101,
        ExcavatorError = 110,
        ExcavatorMissing = 111,
        XmrStakError = 120,
        XmrStakMissing = 121
    }
}
