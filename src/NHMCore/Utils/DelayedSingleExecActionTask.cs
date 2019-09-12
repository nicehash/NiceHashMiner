using NHM.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    /// <summary>
    /// This function makes sure that a single action is executed after a given delay. On each execute call it will reset the delay timer e.g. on each key press event the action execution delay time will be restarted.
    /// </summary>
    class DelayedSingleExecActionTask
    {
        Task _pendingTask = null;
        // TODO make sure you dispose of the tokens
        CancellationTokenSource _cts = null;
        readonly TimeSpan _delayedExecutionTime;
        readonly Action _action = null;

        public DelayedSingleExecActionTask(Action action, TimeSpan delayedExecutionTime)
        {
            _action = action;
            _delayedExecutionTime = delayedExecutionTime;
        }

        public void ExecuteDelayed(CancellationToken token)
        {
            // don't await the task. here we want to just fire it in the background
            _ = ExecuteDelayedTask(token);
        }

        public async Task ExecuteDelayedTask(CancellationToken token)
        {
            // our action can be re-entered
            var previousCts = _cts;
            var newCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _cts = newCts;

            if (previousCts != null)
            {
                // cancel the previous session and wait for its termination
                previousCts.Cancel();
                try
                {
                    await _pendingTask;
                }
                catch (OperationCanceledException)
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    Logger.ErrorDelayed("DelayedSingleExecActionTask", $"ExecuteDelayed {e.Message}", _delayedExecutionTime);
                }
            }
            if (token.IsCancellationRequested) return;
            _pendingTask = ExecuteAction(newCts.Token);
            await _pendingTask;
        }

        // delay exec action logic
        async Task ExecuteAction(CancellationToken token)
        {
            try
            {
                await Task.Delay(_delayedExecutionTime, token);
                if (token.IsCancellationRequested) return;
                // exec if not canceled
                _action();
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
            catch (Exception e)
            {
                Logger.ErrorDelayed("DelayedSingleExecActionTask", $"ExecuteAction {e.Message}", _delayedExecutionTime);
            }
        }
    }
}
