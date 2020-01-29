using Microsoft.Win32;
using NHM.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NHMCore.Utils
{
    public static class WindowsDefender
    {
        private static int lastIndex = 0;

        // TODO current dir is not same as Root, Launcher and all that
        //private static string AppDir => Directory.GetCurrentDirectory();
        private static string AppDir => Paths.Root;
        public static bool IsAlreadySet()
        {
            try
            {
                var cwd = AppDir;
                using (var userRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                {
                    var registrySubKeys = userRegistryKey.GetValueNames().Where(value => value.Contains("WindowsDefenderExclusion")).ToList();
                    foreach (var regVal in registrySubKeys)
                    {
                        if (userRegistryKey.GetValue(regVal).ToString() == cwd)
                        {
                            return true;
                        }
                    }
                    if (registrySubKeys.Count != 0)
                    {
                        lastIndex = Convert.ToInt32(registrySubKeys.Last().Substring("WindowsDefenderExclusion".Length)) + 1;
                    }
                    return false;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //unable to say - better let the user to do it manually
                return true;
            }
        }

        public static bool AddException()
        {
            try
            {
                var fileName = Paths.AppRootPath("AddWindowsDefenderExclusion.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = $"add {AppDir}",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                startInfo.WindowStyle = ProcessWindowStyle.Hidden; // used for hidden window
                using (var addDefenderException = new Process { StartInfo = startInfo })
                {
                    addDefenderException.Start();
                    addDefenderException?.WaitForExit(10 * 1000);
                    if (addDefenderException?.ExitCode != 0)
                    {
                        Logger.Info("NICEHASH", "addDefenderException returned error code: " + addDefenderException.ExitCode);
                    }
                    else
                    {
                        var cwd = AppDir;
                        using (var userRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                        {
                            userRegistryKey.SetValue("WindowsDefenderExclusion" + lastIndex, cwd);
                            Logger.Info("NICEHASH", "addDefenderException all OK");
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", $"AddDefenderException error: {ex.Message}");
            }
            return false;
        }
    }
}
