﻿using Microsoft.Win32;
using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public class Helpers : PInvokeHelpers
    {
        private static readonly bool Is64BitProcess = (IntPtr.Size == 8);
        public static bool Is64BitOperatingSystem = Is64BitProcess || InternalCheckIsWow64();

        public static readonly bool IsElevated;
        private static int StartEpoch = 0;


        static Helpers()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            IsElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            StartEpoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using var p = Process.GetCurrentProcess();
                return IsWow64Process(p.Handle, out var retVal) && retVal;
            }
            return false;
        }

        public static uint GetIdleTime()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        public static void DisableWindowsErrorReporting(bool en)
        {
            //bool failed = false;

            Logger.Info("NICEHASH", "Trying to enable/disable Windows error reporting");

            // CurrentUser
            try
            {
                using var rk = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\Windows Error Reporting");
                if (rk == null)
                {
                    Logger.Info("NICEHASH", "Unable to open SubKey.");
                    return;
                }
                var o = rk.GetValue("DontShowUI");
                if (o == null)
                {
                    Logger.Info("NICEHASH", "Registry key not found .. creating one..");
                    rk.CreateSubKey("DontShowUI", RegistryKeyPermissionCheck.Default);
                    Logger.Info("NICEHASH", "Setting register value to 1..");
                    rk.SetValue("DontShowUI", en ? 1 : 0);
                    return;
                }
                var val = (int)o;
                Logger.Info("NICEHASH", $"Current DontShowUI value: {val}");

                if (val == 0 && en)
                {
                    Logger.Info("NICEHASH", "Setting register value to 1.");
                    rk.SetValue("DontShowUI", 1);
                }
                else if (val == 1 && !en)
                {
                    Logger.Info("NICEHASH", "Setting register value to 0.");
                    rk.SetValue("DontShowUI", 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", $"Unable to access registry. Error: {ex.Message}");
            }
        }

        public static string FormatSpeedOutput(IEnumerable<Hashrate> hashrates)
        {
            if (hashrates.Count() > 0)
            {
                var hashrateStrings = hashrates.Select(hashrate => hashrate.ToString());
                return string.Join(" + ", hashrateStrings);
            }
            return "N/A";
        }

        public static string FormatSpeedOutput(IEnumerable<(AlgorithmType type, double speed)> speedPairs)
        {
            var hashrates = speedPairs.Select(pair => new Hashrate(pair.speed, pair.type));
            return FormatSpeedOutput(hashrates);
        }

        // Checking the version using >= will enable forward compatibility, 
        // however you should always compile your code on newer versions of
        // the framework to ensure your app works the same.
        private static bool Is45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
            {
                //return "4.6 or later";
                return true;
            }
            if ((releaseKey >= 379893))
            {
                //return "4.5.2 or later";
                return true;
            }
            if ((releaseKey >= 378675))
            {
                //return "4.5.1 or later";
                return true;
            }
            if ((releaseKey >= 378389))
            {
                //return "4.5 or later";
                return true;
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            //return "No 4.5 or later version detected";
            return false;
        }

        public static bool Is45NetOrHigher()
        {
            using var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\");
            return ndpKey?.GetValue("Release") != null && Is45DotVersion((int)ndpKey.GetValue("Release"));
        }

        public static bool IsConnectedToInternet()
        {
            bool returnValue;
            try
            {
                returnValue = InternetGetConnectedState(out _, 0);
            }
            catch
            {
                returnValue = false;
            }
            return returnValue;
        }

        // parsing helpers
        public static int ParseInt(string text)
        {
            return int.TryParse(text, out var tmpVal) ? tmpVal : 0;
        }

        public static long ParseLong(string text)
        {
            return long.TryParse(text, out var tmpVal) ? tmpVal : 0;
        }

        public static double ParseDouble(string text)
        {
            try
            {
                var parseText = text.Replace(',', '.');
                return double.Parse(parseText, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        public static void VisitUrlLink(string url)
        {
            try
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true
                };
                using var _p = Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", "VisitLink error: " + ex.Message);
            }
        }

        public static async Task<(bool isUploaded, string dumpUUID, string uploadUrl)> CreateAndUploadLogReport()
        {
            try
            {
                var tmpZipPath = Paths.RootPath($"tmp._archive_logs.zip");
                // Delete old archive
                try
                {
                    File.Delete(tmpZipPath);
                }
                catch (Exception ex)
                {
                    Logger.Error("Log-Report", $"Unable to delete log archive: {ex.Message}");
                }

                var (dumpUUID, uploadUrl) = CreateSystemDumpUrl();
                // Create archive
                if (!CreateLogArchive()) return (false, dumpUUID, uploadUrl);

                // Upload archive
                var isUploaded = await UploadLogArchive(tmpZipPath, uploadUrl);
                return (isUploaded, dumpUUID, uploadUrl);
            }
            catch (Exception ex)
            {
                Logger.Error("Log-Report", ex.Message);
                return (false, "", "");
            }
        }


        private static bool CreateLogArchive()
        {
            try
            {
                var exePath = Paths.RootPath("CreateLogReport.exe");
                var startLogInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = true,
                    Arguments = Path.GetFileName(Paths.AppRoot),
                    CreateNoWindow = true
                };
                Logger.Info("Log-Report", $"Created log report with: {startLogInfo.FileName} - arguments: {startLogInfo.Arguments}");
                using var doCreateLog = Process.Start(startLogInfo);
                doCreateLog.WaitForExit(10 * 1000);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Log-Report", $"Unable to create log archive: {ex.Message}");
                return false;
            }
        }

        private static (string uuid, string url) CreateSystemDumpUrl()
        {
            var uuid = System.Guid.NewGuid().ToString();
            var rigId = ApplicationStateManager.RigID();
            var url = $"https://nhos.nicehash.com/nhm-dump/{rigId}-{uuid}.zip";
            return (uuid, url);
        }

        private static async Task<bool> UploadLogArchive(string tmpArchivePath, string uploadUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                using var stream = File.OpenRead(tmpArchivePath);
                using var content = new StreamContent(stream);
                using var response = await httpClient.PutAsync(uploadUrl, content);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Log-Report", $"Unable to post log archive: {ex.Message}");
                return false;
            }
        }
        public static int GetElapsedSecondsSinceStart()
        {
            var elapsed = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - StartEpoch;
            return elapsed;
        }
        public static IPAddress GetLocalIP()
        {
            return IPAddress.Parse(Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString()) ?? IPAddress.None;
        }

        public static void CreateRunOnStartupBackup()
        {
            try
            {
                var backupPath = Paths.ConfigsPath(".runOnStartup.txt");
                using (var writer = new StreamWriter(backupPath))
                {
                    writer.Write(MiscSettings.Instance.RunAtStartup.ToString());
                }

                Logger.Info("RunOnStartupBackup", $"Created RunOnStartupBackup file");
            }
            catch (Exception ex)
            {
                Logger.Error("RunOnStartupBackup", $"Unable to create RunOnStartupBackup file: {ex.Message}");
            }
        }

        public static bool GetRunOnStartupBackupValue()
        {
            try
            {
                var backupPath = Paths.ConfigsPath(".runOnStartup.txt");
                using (var reader = new StreamReader(backupPath))
                {
                   return Convert.ToBoolean(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetRunOnStartupBackup", $"Unable to read RunOnStartupBackup file: {ex.Message}");
                return false;
            }
        }
    }
}
