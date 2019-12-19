using NHM.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class TaskHelpers
    {
        //https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.completedtask?redirectedfrom=MSDN&view=netframework-4.8#System_Threading_Tasks_Task_CompletedTask
        // net45 doens't support this so make our own
        public static Task CompletedTask { get; } = Task.Delay(0);

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
            return CompletedTask;
        }
    }
}
