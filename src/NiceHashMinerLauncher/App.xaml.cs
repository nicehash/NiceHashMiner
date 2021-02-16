//#define DELETE_NON_CURRENT_APPS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NiceHashMiner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string GetRootPath(params string[] subPaths)
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var paths = new List<string> { rootPath };
            foreach (var subPath in subPaths) paths.Add(subPath);
            var ret = Path.Combine(paths.ToArray());
            return ret;
        }
        private static bool IsDoFile(string fileName)
        {
            try
            {
                return File.Exists(GetRootPath(fileName));
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

        private static bool IsUpdate()
        {
            return IsDoFile(@"do.update");
        }

        private static void ClearAllXXFiles(string pattern)
        {
            var path = GetRootPath();
            var doFiles = Directory.GetFiles(path, pattern);
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

        private static void ClearAllDoFiles()
        {
            ClearAllXXFiles("do.*");
        }
        private static void ClearAllTmpFiles()
        {
            ClearAllXXFiles("tmp.*");
        }

        private static Mutex _mutex = null;

        static readonly string[] AppExeNames = new string[] { "NiceHashMiner.exe", "app_nhm.exe" };
        private static (string appDir, string appPath, Version version) GetLatestApp()
        {
            var path = GetRootPath();
            var appDirs = Directory.GetDirectories(path, "app*");

            if (appDirs.Length > 0)
            {
                Version latest = null;
                string retAppDir = null;
                string retAppExe = null;
                foreach (var appDir in appDirs)
                {
                    foreach (var appExe in AppExeNames)
                    {
                        try
                        {
                            var lastIndex = appDir.LastIndexOf("app_");
                            var verStr = appDir.Substring(lastIndex, appDir.Length - lastIndex).Replace("app_", "");
                            var compareVer = new Version(verStr);
                            var exeExists = File.Exists(Path.Combine(appDir, appExe));
                            if ((latest == null || latest < compareVer) && exeExists)
                            {
                                latest = compareVer;
                                retAppDir = appDir;
                                retAppExe = appExe;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                if (retAppDir != null) return (retAppDir, retAppExe, latest);
            }
            // return default app dir
            return ("app", "app_nhm.exe", null);
        }

        private static void DeleteAllExceptCurrentVersion(Version currentVersion)
        {
            if (currentVersion == null) return;
            var path = GetRootPath();
            var appDirs = Directory.GetDirectories(path, "app*");

            foreach (var appDir in appDirs)
            {
                try
                {
                    var lastIndex = appDir.LastIndexOf("app_");
                    var verStr = appDir.Substring(lastIndex, appDir.Length - lastIndex).Replace("app_", "");
                    var compareVer = new Version(verStr);
                    if (compareVer != currentVersion)
                    {
                        Directory.Delete(appDir, true);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static string GetLatestUpdater()
        {
            var path = GetRootPath("updaters");
            const string updaterPrefix = "nhm_windows_";
            const string updaterPrefixPart = "updater_";
            var updaters = Directory.GetFiles(path, $"{updaterPrefix}*.*");
            if (updaters.Length > 0)
            {
                Version latest = null;
                string retUpdater = null;
                foreach (var updater in updaters)
                {
                    try
                    {
                        var updaterNoExtension = Path.GetFileNameWithoutExtension(updater);
                        var lastIndex = updaterNoExtension.LastIndexOf(updaterPrefix);
                        var verStr = updaterNoExtension.Replace(updaterPrefix, "").Replace(updaterPrefixPart, ""); ;
                        var compareVer = new Version(verStr);
                        if ((latest == null || latest < compareVer))
                        {
                            latest = compareVer;
                            retUpdater = updater;
                        }
                    }
                    catch (Exception)
                    {
                    }

                }
                if (retUpdater != null) return retUpdater;
            }
            // no updater
            return null;
        }

        public static string GetHashString()
        {
            var appPathBytes = Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().Location);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in appPathBytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            ClearAllDoFiles();
            // Set shutdown mode back to default
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var isUpdater = Environment.GetCommandLineArgs().Contains("-update");
            var isUpdated = Environment.GetCommandLineArgs().Contains("-updated");
            if (isUpdater)
            {
                var latestUpdaterFile = GetLatestUpdater();
                if (latestUpdaterFile == null)
                {
                    var restartProcessUpdatefailed = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = GetRootPath("NiceHashMiner.exe"),
                            WorkingDirectory = GetRootPath(),
                            Arguments = "-updateFailed",
                            WindowStyle = ProcessWindowStyle.Normal
                        }
                    };
                    restartProcessUpdatefailed.Start();
                    // shutdown
                    Shutdown();
                    return;
                }
                var updaterFile = GetRootPath("updaters", latestUpdaterFile);
                var isZip = updaterFile.EndsWith(".zip");

                //await Task.Delay(5000);
                if (isZip)
                {
                    var pluginPackagesPath = GetRootPath("plugins_packages");
                    if (Directory.Exists(pluginPackagesPath)) Directory.Delete(pluginPackagesPath, true);

                    var progWindow = new UpdateProgress();
                    progWindow.Show();
                    var isOk = await UnzipFileAsync(updaterFile, GetRootPath(), progWindow.Progress, CancellationToken.None);
                    progWindow.Close();
                    await Task.Delay(500);
                    // TODO if something goes wrong just restore the current exe process file
                    var restartProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = GetRootPath("NiceHashMiner.exe"),
                            WorkingDirectory = GetRootPath(),
                            Arguments = "-updated",
                            WindowStyle = ProcessWindowStyle.Normal
                        }
                    };
                    restartProcess.Start();
                }
                else
                {
                    var startUpdater = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = updaterFile,
                            WorkingDirectory = GetRootPath(),
                            WindowStyle = ProcessWindowStyle.Normal
                        }
                    };
                    startUpdater.Start();
                    // TODO we should wait and check if we have restarted NiceHash Miner
                }

                // shutdown
                Shutdown();
                return;
            }
            if (isUpdated) await Task.Delay(500);  // so we release the temp files

            await WindowsUptimeCheck.DelayUptime();

            bool createdNew = false;
            try
            {
                string appPath = GetHashString();
                _mutex = new Mutex(true, appPath, out createdNew);
                if (!createdNew)
                {
                    //MessageBox.Show("We have detected you are already running NiceHash Miner. Only a single instance should be running at a time.", "NiceHash Miner Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // shutdown
                    Shutdown();
                    return;
                }

                ClearAllTmpFiles();

                // TODO pass parent process PID
                var (latestAppDir, latestAppExe, version) = GetLatestApp();
#if DELETE_NON_CURRENT_APPS
#error "Comment this line if you really wish to execute apps cleanup"
                DeleteAllExceptCurrentVersion(version); // cleanup on next release
#endif
                var nhmApp = GetRootPath(latestAppDir, latestAppExe);
                var args = $"-lc -PID{Process.GetCurrentProcess().Id}";
                if (isUpdated) args += " -updated";
                var startInfo = new ProcessStartInfo
                {
                    FileName = nhmApp,
                    Arguments = args,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                var run = true;
                const int maxRestartCount = 3;
                int restartCount = 0;
                const int minRestartTimeInSeconds = 30;
                while (run)
                {
                    var startTime = DateTime.UtcNow;
                    run = false;
                    try
                    {
                        using (var niceHashMiner = new Process { StartInfo = startInfo })
                        {
                            var hasStarted = niceHashMiner?.Start();
                            niceHashMiner?.WaitForExit();
                            // TODO 
                            Console.WriteLine(niceHashMiner.ExitCode);
                            //in case of crash try to restart the program
                            if (niceHashMiner.ExitCode != 0)
                            {
                                var endTime = DateTime.UtcNow;

                                string path = GetRootPath("logs", "watchdogLog.txt");
                                if (!File.Exists(path))
                                {
                                    using (var sw = File.CreateText(path))
                                    {
                                        sw.WriteLine($"Exit code: {niceHashMiner.ExitCode} ---- {endTime}");
                                    }
                                }
                                else
                                {
                                    using (var sw = File.AppendText(path))
                                    {
                                        sw.WriteLine($"Exit code: {niceHashMiner.ExitCode} ---- {endTime}");
                                    }
                                }
                                //try to re-run the NHM
                                run = true;

                                //check if too many restarts
                                var elapsedSeconds = (endTime - startTime).TotalSeconds;
                                if (elapsedSeconds < minRestartTimeInSeconds)
                                {
                                    restartCount++;
                                }
                                else
                                {
                                    restartCount = 0;
                                }
                                if (restartCount >= maxRestartCount)
                                {
                                    using (var sw = File.AppendText(path))
                                    {
                                        sw.WriteLine($"Too many restarts! Closing nhm");
                                    }
                                    MessageBox.Show("NHM experienced too many crashes recently, therefore it will close itself", "Too many restarts");
                                    run = false;
                                }
                            }
                            else
                            {
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
                                else if (IsUpdate())
                                {
                                    run = true; // mark to false if updating doesn't fail
                                    ClearAllDoFiles();
                                    var exePath = Assembly.GetExecutingAssembly().Location;
                                    var randomPart = DateTime.UtcNow.Millisecond;
                                    var tmpLauncher = GetRootPath($"tmp.nhm_updater_{randomPart}.exe");
                                    File.Copy(exePath, tmpLauncher, true);
                                    var doUpdate = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = tmpLauncher,
                                            Arguments = "-update",
                                            WindowStyle = ProcessWindowStyle.Normal
                                        }
                                    };
                                    var updateStarted = doUpdate.Start();
                                    run = !updateStarted; // set if we are good
                                }
                                else { /*ELSE*/ }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                // shutdown
                Shutdown();
                return;
            }
            finally
            {
                if (createdNew) _mutex?.ReleaseMutex();
            }
        }

        public static async Task<bool> UnzipFileAsync(string zipLocation, string unzipLocation, IProgress<int> progress, CancellationToken stop)
        {
            try
            {
                using (var archive = ZipFile.OpenRead(zipLocation))
                {
                    float entriesCount = archive.Entries.Count;
                    float extractedEntries = 0;
                    foreach (var entry in archive.Entries)
                    {
                        if (stop.IsCancellationRequested) break;

                        extractedEntries += 1;
                        var isDirectory = entry.Name == "";
                        if (isDirectory) continue;

                        var prog = ((extractedEntries / entriesCount) * 100.0f);
                        progress?.Report((int)prog);

                        var extractPath = Path.Combine(unzipLocation, entry.FullName);
                        var dirPath = Path.GetDirectoryName(extractPath);
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                        }
                        //entry.ExtractToFile(extractPath, true);

                        using (var zipStream = entry.Open())
                        using (var fileStream = new FileStream(extractPath, FileMode.Create, FileAccess.Write)) // using (var fileStream = new FileStream(extractPath, FileMode.CreateNew))
                        {
                            await zipStream.CopyToAsync(fileStream);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while unzipping file: {e.Message}");
                return false;
            }
        }
    }
}
