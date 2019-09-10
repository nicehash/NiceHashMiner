using NHM.Common;
using System;

namespace NHMCore.Mining.IdleChecking
{
    internal abstract class IdleChecker : IDisposable
    {
        public event EventHandler<IdleChangedEventArgs> IdleStatusChanged;

        public abstract void StartChecking();

        protected void FireStatusEvent(bool isIdle)
        {
            Logger.Info("IdleChecker", $"Idle status changed to {isIdle}");
            IdleStatusChanged?.Invoke(null, new IdleChangedEventArgs(isIdle));
        }

        #region IDisposable Implementation

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IdleChecker()
        {
            Dispose(false);
        }

        #endregion
    }
}
