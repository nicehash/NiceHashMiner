using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices.Querying
{
    public static class DeviceDetectionPrinter
    {
        public static async Task<T>GetDeviceDetectionResultAsync<T>(string args, int milliseconds = (30 * 1000)) where T : class
        {
            string readData = "";
            T result = null;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "DeviceDetectionPrinter.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                using (var run = new Process() { StartInfo = startInfo, EnableRaisingEvents = true })
                using (var ct = new CancellationTokenSource(milliseconds))
                {
                    if (!run.Start())
                    {
                        throw new InvalidOperationException("Could not start process: " + run);
                    }
                    Action<string> stopProcess = (string stopFrom) => {
                        try
                        {
                            var isRunning = !run?.HasExited ?? false;
                            if (!isRunning) return;
                            run.CloseMainWindow();
                            var hasExited = run?.WaitForExit(1000) ?? false;
                            if (!hasExited) run.Kill();
                        }
                        catch (Exception e)
                        {
                            // TODO log
                        }
                    };
                    ct.Token.Register(() => stopProcess("from cancel token"));
                    readData = await run.StandardOutput.ReadToEndAsync();
                    stopProcess("after read end");
                    result = JsonConvert.DeserializeObject<T>(readData, Globals.JsonSettings);
                    if (result == null && !string.IsNullOrEmpty(readData))
                    {
                        Logger.Debug("DeviceDetectionPrinter", $"result is NULL readData='{readData}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DeviceDetectionPrinter", $"threw Exception: '{ex.Message}'. ReadData '{readData}'");
            }

            return result;
        }
    }
}
