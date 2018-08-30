using System;
using Microsoft.Win32;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.IdleChecking
{
    internal static class IdleCheckerManager
    {
        private static IdleChecker _checker;

        public static void StartIdleCheck(IdleCheckType type, EventHandler<IdleChangedEventArgs> evnt)
        {
            _checker?.Dispose();

            if (type == IdleCheckType.SessionLock)
                _checker = new SessionLockChecker();
            else
                _checker = new InputTimeoutChecker();

            _checker.IdleStatusChanged += evnt;
            _checker.StartChecking();
        }
    }
}
