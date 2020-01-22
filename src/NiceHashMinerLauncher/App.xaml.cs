using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace NiceHashMiner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static bool IsDoFile(string fileName)
        {
            try
            {
                return File.Exists(fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"IsRunAsAdmin error: {e.Message}");
            }
            return false;
        }

        private static bool IsRunAsAdmin()
        {
            return IsDoFile(@"do.runasadmin");
        }

        private static bool IsRestart()
        {
            return IsDoFile(@"do.restart");
        }

        private static void ClearAllDoFiles()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var doFiles = Directory.GetFiles(path, "do.*");
            foreach (var doFile in doFiles)
            {
                try
                {
                    File.Delete(doFile);
                }
                catch (Exception)
                {
                }
            }
        }

        private static string GetLatestApp()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var appDirs = Directory.GetDirectories(path, "app*");
            if (appDirs.Length > 0)
            {
                Version latest = null;
                string retAppdir = null;
                foreach (var appDir in appDirs)
                {
                    try
                    {
                        var lastIndex = appDir.LastIndexOf("app_");
                        var verStr = appDir.Substring(lastIndex, appDir.Length - lastIndex).Replace("app_", "");
                        var compareVer = new Version(verStr);
                        var exeExists = File.Exists(Path.Combine(appDir, "NiceHashMiner.exe"));
                        if ((latest == null || latest < compareVer) && exeExists)
                        {
                            latest = compareVer;
                            retAppdir = appDir;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    
                }
                if (retAppdir != null) return retAppdir;
            }
            // return default app dir
            return "app";
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ClearAllDoFiles();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // TODO add override latest version run (maybe but rather not)

            // TODO pass parent process PID
            var latestAppDir = GetLatestApp();
            var startInfo = new ProcessStartInfo
            {
                FileName = $@".\{latestAppDir}\NiceHashMiner.exe",
                Arguments = "-lc"
            };
            var run = true;
            while (run)
            {
                run = false;
                using (var niceHashMiner = new Process { StartInfo = startInfo })
                {
                    var hasStarted = niceHashMiner?.Start();
                    niceHashMiner?.WaitForExit();
                    // TODO 
                    Console.WriteLine(niceHashMiner.ExitCode);
                    // if exit code is 0 then check runasadmin or restart
                    if (IsRunAsAdmin())
                    {
                        RunAsAdmin.SelfElevate();
                    }
                    else if (IsRestart())
                    {
                        ClearAllDoFiles();
                        run = true;
                    }
                }
            }

            // shutdown
            Shutdown();
            return;
        }
    }
}
