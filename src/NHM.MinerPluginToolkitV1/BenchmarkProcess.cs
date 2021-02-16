using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1
{
    // This one just reads from std out and err
    public class BenchmarkProcess
    {
        public BenchmarkProcess(Process p)
        {
            Handle = p;
        }

        public BenchmarkProcess(string binPath, string workingDir, string commandLine, Dictionary<string, string> environmentVariables = null)
        {
            Handle = MinerToolkit.CreateBenchmarkProcess(binPath, workingDir, commandLine, environmentVariables);
        }

        public delegate BenchmarkResult CheckDataFun(string data);
        public CheckDataFun CheckData { get; set; }
        public Dictionary<string, string> BenchmarkExceptions { get; set; }
        public Process Handle { get; }
        public BenchmarkResult Result { get; private set; } = default(BenchmarkResult);
        public bool Success { get; private set; } = default(bool);

        private bool _exitCalled = false;

        private void BenchmarkOutputErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (Success || CheckData == null || string.IsNullOrEmpty(e.Data) || _exitCalled)
            {
                return;
            }

            var errorMessage = BenchmarkProcessSettings.IsBenchmarkExceptionLine(e.Data, BenchmarkExceptions);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Result = new BenchmarkResult { ErrorMessage = errorMessage };
                TryExit();
                return;
            }

            Result = CheckData(e.Data);
            if (Result.Success)
            {
                TryExit();
            }
        }

        public void TryExit()
        {
            try
            {
                var isRunning = Process.GetProcessById(Handle.Id) != null;
                if (isRunning)
                {
                    try
                    {
                        Handle.OutputDataReceived -= BenchmarkOutputErrorDataReceived;
                        Handle.ErrorDataReceived -= BenchmarkOutputErrorDataReceived;
                        Handle.Kill();
                    }
                    catch
                    { } // TODO gotta catch them all!!!
                }
            }
            catch { }
        }

        public Task<BenchmarkResult> Execute(CancellationToken stop)
        {
            var tcs = new TaskCompletionSource<BenchmarkResult>();
            // TODO throw if handle is not initialized
            //if (Handle == null)
            //{
            //    return Task.;
            //}

            //tcs.SetCanceled()

            Handle.Exited += (s, ea) =>
            {
                _exitCalled = true;
                tcs.SetResult(Result);
            };
            Handle.OutputDataReceived += BenchmarkOutputErrorDataReceived;
            Handle.ErrorDataReceived += BenchmarkOutputErrorDataReceived;

            // TODO BeforeStart

            if (!Handle.Start())
            {
                // TODO maybe not throw, maybe just return failed result instead
                throw new InvalidOperationException("Could not start process: " + Handle);
            }

            // TODO AfterStart

            Handle.BeginOutputReadLine();
            Handle.BeginErrorReadLine();
            stop.Register(() =>
            {
                TryExit();
            });

            return tcs.Task;
        }
    }
}
