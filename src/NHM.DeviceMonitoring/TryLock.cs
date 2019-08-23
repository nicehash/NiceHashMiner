using System;
using System.Threading;

namespace NHM.DeviceMonitoring
{
    internal class TryLock : IDisposable
    {
        private object locked;

        public bool HasAcquiredLock { get; private set; }

        public TryLock(object obj)
        {
            if (Monitor.TryEnter(obj))
            {
                HasAcquiredLock = true;
                locked = obj;
            }
        }

        public void Dispose()
        {
            if (HasAcquiredLock)
            {
                Monitor.Exit(locked);
                locked = null;
                HasAcquiredLock = false;
            }
        }
    }
}
