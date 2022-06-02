using NHM.Common;
using System;
using System.Diagnostics;
using System.IO;

namespace NHMCore.Utils
{
    public static class RunAsAdmin
    {
        public static void SelfElevate()
        {
            try
            {
                var current = Process.GetCurrentProcess();
                Execute(current.Id, NHMApplication.ExecutablePath);
            }
            catch (Exception)
            {
            }
        }

        public static void Execute(int pid, string path)
        {
            if (Launcher.IsLauncher)
            {
                try
                {
                    File.Create(Paths.RootPath("do.runasadmin"));
                    ApplicationStateManager.ExecuteApplicationExit();
                }
                catch (Exception e)
                {
                    Logger.Error("NICEHASH", $"RunAsAdmin IsLauncher error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    using var runAsAdmin = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Paths.RootPath("runnhmasadmin.exe"),
                            Arguments = $"{pid} \"{path}\"",
                            Verb = "runas",
                            UseShellExecute = true,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden, // used for hidden window
                        }
                    };
                    runAsAdmin.Start();
                    runAsAdmin.WaitForExit();
                }
                catch (Exception ex)
                {
                    Logger.Error("NICEHASH", $"RunAsAdmin error: {ex.Message}");
                }
            }
        }
    }
}
