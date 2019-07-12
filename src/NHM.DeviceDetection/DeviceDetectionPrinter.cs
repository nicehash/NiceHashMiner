using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NHM.Common;

namespace NHM.DeviceDetection
{

    internal static class DeviceDetectionPrinter
    {
        #region JSON settings
        private static readonly JsonSerializerSettings _jsonSettings;

        static DeviceDetectionPrinter()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
            };
        }
        #endregion JSON settings

        public static async Task<(string readData, T parsed)> GetDeviceDetectionResultAsync<T>(string args, int milliseconds = (30 * 1000)) where T : class
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
                        catch (Exception)
                        {
                            // TODO log
                        }
                    };
                    ct.Token.Register(() => stopProcess("from cancel token"));
                    readData = await run.StandardOutput.ReadToEndAsync();
                    stopProcess("after read end");
                    result = JsonConvert.DeserializeObject<T>(readData, _jsonSettings);
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

            return (readData, result);
        }
    }
}
