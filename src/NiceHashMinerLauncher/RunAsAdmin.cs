using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NiceHashMiner
{
    internal static class RunAsAdmin
    {
        public static void SelfElevate()
        {
            try
            {
                var current = Process.GetCurrentProcess();
                Execute(current.Id, Application.ExecutablePath);
            }
            catch (Exception)
            {
            }
        }

        public static void Execute(int pid, string path)
        {
            try
            {
                using var runAsAdmin = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"runnhmasadmin.exe",
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
                Console.WriteLine($"RunAsAdmin error: {ex.Message}");
            }
        }
    }
}
