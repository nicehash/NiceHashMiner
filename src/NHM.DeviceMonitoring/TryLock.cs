using System;
using System.Threading;

namespace NHM.DeviceMonitoring
{
    internal class TryLock : IDisposable
    {
        private object locked;

        public bool HasLock { get; private set; }

        public TryLock(object obj)
        {
            if (Monitor.TryEnter(obj))
            {
                HasLock = true;
                locked = obj;
            }
        }

        public void Dispose()
        {
            if (HasLock)
            {
                Monitor.Exit(locked);
                locked = null;
                HasLock = false;
            }
        }
    }
}
