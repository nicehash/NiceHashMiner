using System;
using Microsoft.Win32;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.IdleChecking
{
    internal static class IdleCheckerManager
    {
        private static IdleChecker _checker;

        static IdleCheckerManager()
        {
            //StartLockCheck();
        }

        public static void StartIdleCheck(IdleCheckType type, EventHandler<IdleChangedEventArgs> evnt)
        {
            if (type == IdleCheckType.SessionLock)
                _checker = new SessionLockChecker();

            _checker.IdleStatusChanged += evnt;
            _checker.StartChecking();
        }
    }
}
