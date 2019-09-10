using Microsoft.Win32;

namespace NHMCore.Mining.IdleChecking
{
    internal class SessionLockChecker : IdleChecker
    {
        public override void StartChecking()
        {
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
        }

        private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            FireStatusEvent(e.Reason == SessionSwitchReason.SessionLock);
        }

        protected override void Dispose(bool disposing)
        {
            // This must be called to prevent mem leak
            SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;
        }
    }
}
