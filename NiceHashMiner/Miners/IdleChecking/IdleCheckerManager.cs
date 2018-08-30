using System;
using Microsoft.Win32;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.IdleChecking
{
    internal static class IdleCheckerManager
    {
        public static event EventHandler<IdleChangedEventArgs> IdleStatusChanged;

        static IdleCheckerManager()
        {
            //StartLockCheck();
        }

        public static void StartIdleCheck(IdleCheckType type)
        {
            if (type == IdleCheckType.SessionLock)
                StartLockCheck();
            else
                StopLockCheck();
        }

        private static void StartLockCheck()
        {
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
        }

        private static void StopLockCheck()
        {
            SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;
        }

        private static void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            FireStatusEvent(e.Reason == SessionSwitchReason.SessionLock);
        }

        private static void FireStatusEvent(bool isIdle)
        {
            Helpers.ConsolePrint("[IDLE]", $"Idle status changed to idling = {isIdle}");
            IdleStatusChanged?.Invoke(null, new IdleChangedEventArgs(isIdle));
        }
    }
}
