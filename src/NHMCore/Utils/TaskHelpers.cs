using NHM.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class TaskHelpers
    {
        public static Task TryDelay(TimeSpan delay, CancellationToken token)
        {
            return TryDelay((int)delay.TotalMilliseconds, token);
        }

        public static Task TryDelay(int millisecondsDelay, CancellationToken token)
        {
            try
            {
                return Task.Delay(millisecondsDelay, token);
            }
            catch (Exception e)
            {
                // catch all
                Logger.Debug("TaskHelpers", $"catch block {e.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
