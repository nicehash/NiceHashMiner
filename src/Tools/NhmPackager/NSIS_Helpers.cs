using NHM.Common;
using System.Diagnostics;
using System.IO;

namespace NhmPackager
{
    internal static class NSIS_Helpers
    {

        internal static bool Exec7ZipPackage(string releasePath, string distPath)
        {
            Logger.Info("7z-package", "Preparing nhm.7z package");
            Logger.Info("7z-package", distPath);
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "7z.exe",
                    Arguments = $"a -t7z -bd nhm.7z {releasePath}\\*",
                    WorkingDirectory = distPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            var ok = proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Logger.Info("7z-package", line);
            }
            return ok && proc.ExitCode == 0;
        }

        internal static void PrepareUninstaller(string nsisDirPath)
        {
            var uninstallerNsiScript = Path.Combine(nsisDirPath, "uninstaller.nsi");
            CallMakeNSIS_Exe(uninstallerNsiScript);
            var uninstallerExe = Path.Combine(nsisDirPath, "uninstaller.exe");
            UnpackUninstaller(uninstallerExe);
            File.Delete(uninstallerExe);
        }

        private static void UnpackUninstaller(string uninstallerExePath)
        {
            var cwdPath = Path.GetDirectoryName(uninstallerExePath);
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "7z.exe",
                    Arguments = $"x {uninstallerExePath}",
                    WorkingDirectory = cwdPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            var ok = proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Logger.Info("7z-unpack", line);
            }
            //return ok && proc.ExitCode == 0;
        }

        internal static void CallMakeNSIS_Exe(string scriptPath)
        {
            var cwdPath = Path.GetDirectoryName(scriptPath);
            Logger.Info("MakeNsis", $"script '{scriptPath}'");
            Logger.Info("MakeNsis", $"cwd '{cwdPath}'");
            using var makeNsis = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "makensis.exe",
                    Arguments = $"\"{scriptPath}\"",
                    WorkingDirectory = cwdPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            var ok = makeNsis.Start();
            while (!makeNsis.StandardOutput.EndOfStream)
            {
                string line = makeNsis.StandardOutput.ReadLine();
                Logger.Info("MakeNsis", line);
            }
            //return ok && makeNsis.ExitCode == 0;
        }
    }
}
