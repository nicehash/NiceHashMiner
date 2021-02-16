using NHM.Common;
using System;
using System.Diagnostics;

namespace NHMCore.Utils
{
    public static class AmdModeSwitcher
    {
        public static void SwitchAmdComputeMode()
        {
            try
            {
                var fileName = Paths.AppRootPath("AmdComputeModeSwitcher.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                startInfo.WindowStyle = ProcessWindowStyle.Hidden; // used for hidden window
                using (var amdModeSwitcher = new Process { StartInfo = startInfo })
                {
                    amdModeSwitcher.Start();
                    amdModeSwitcher?.WaitForExit(10 * 1000);
                    if (amdModeSwitcher?.ExitCode != 0)
                    {
                        Logger.Info("NICEHASH", "amdModeSwitcher returned error code: " + amdModeSwitcher.ExitCode);
                    }
                    else
                    {
                        Logger.Info("NICEHASH", "amdModeSwitcher all OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", $"SwitchAmdComputeMode error: {ex.Message}");
            }
        }
    }
}
