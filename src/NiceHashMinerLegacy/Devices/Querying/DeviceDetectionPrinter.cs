using Newtonsoft.Json;
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
            var run = new Process()
            {
                StartInfo =
                {
                    FileName = "DeviceDetectionPrinter.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };
            string readData = "";
            T result = null;
            try
            {
                if (!run.Start())
                {
                    throw new InvalidOperationException("Could not start process: " + run);
                }
                using (var ct = new CancellationTokenSource(milliseconds))
                {
                    ct.Token.Register(() =>
                    {
                        try
                        {
                            run.Kill();
                        }
                        catch { }
                    });
                    readData = await run.StandardOutput.ReadToEndAsync();
                    result = JsonConvert.DeserializeObject<T>(readData, Globals.JsonSettings);
                    if (result == null && !string.IsNullOrEmpty(readData))
                    {
                        Helpers.ConsolePrint("DeviceDetectionPrinter", $"result is NULL readData='{readData}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("DeviceDetectionPrinter", $"threw Exception: '{ex.Message}'. ReadData '{readData}'");
            }
            finally
            {
                run.Dispose();
            }

            return result;
        }
    }
}
