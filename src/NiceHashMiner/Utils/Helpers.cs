using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using NHM.Common.Enums;
using System.Linq;
using NHM.Common;

namespace NiceHashMiner.Utils
{
    internal class Helpers : PInvokeHelpers
    {
        private static readonly bool Is64BitProcess = (IntPtr.Size == 8);
        public static bool Is64BitOperatingSystem = Is64BitProcess || InternalCheckIsWow64();

        public static readonly bool IsElevated;


        static Helpers()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                IsElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (var p = Process.GetCurrentProcess())
                {
                    return IsWow64Process(p.Handle, out var retVal) && retVal;
                }
            }
            return false;
        }

        public static uint GetIdleTime()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint) System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint) Environment.TickCount - lastInPut.dwTime);
        }

        public static void DisableWindowsErrorReporting(bool en)
        {
            //bool failed = false;

            Logger.Info("NICEHASH", "Trying to enable/disable Windows error reporting");

            // CurrentUser
            try
            {
                using (var rk = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\Windows Error Reporting"))
                {
                    if (rk != null)
                    {
                        var o = rk.GetValue("DontShowUI");
                        if (o != null)
                        {
                            var val = (int) o;
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
                        else
                        {
                            Logger.Info("NICEHASH", "Registry key not found .. creating one..");
                            rk.CreateSubKey("DontShowUI", RegistryKeyPermissionCheck.Default);
                            Logger.Info("NICEHASH", "Setting register value to 1..");
                            rk.SetValue("DontShowUI", en ? 1 : 0);
                        }
                    }
                    else
                        Logger.Info("NICEHASH", "Unable to open SubKey.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", $"Unable to access registry. Error: {ex.Message}");
            }
        }

        public static string FormatSpeedOutput(double speed, AlgorithmType algorithmType, string separator = " ")
        {
            string ret = "";

            if (speed < 1000)
                ret = (speed).ToString("F3", CultureInfo.InvariantCulture) + separator;
            else if (speed < 100000)
                ret = (speed * 0.001).ToString("F3", CultureInfo.InvariantCulture) + separator + "k";
            else if (speed < 100000000)
                ret = (speed * 0.000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "M";
            else
                ret = (speed * 0.000000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "G";

            var unit = GetUnitForAlgorithmType(algorithmType);        
            return ret + unit;
        }

        public static string GetUnitForAlgorithmType(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                //case AlgorithmType.Equihash:
                case AlgorithmType.ZHash:
                case AlgorithmType.Beam:
                    return "Sol/s";
                case AlgorithmType.GrinCuckaroo29:
                case AlgorithmType.GrinCuckatoo31:
                case AlgorithmType.CuckooCycle:
                    return "G/s";
                default:
                    return "H/s";
            }
        }

        public static string GetNameFromAlgorithmTypes(params AlgorithmType[] ids)
        {
            var names = ids.Where(id => (int)id > -1).Select(id => Enum.GetName(typeof(AlgorithmType), id));
            return string.Join("+", names);
        }

        // TODO disable params for ids
        public static string FormatDualSpeedOutput(double primarySpeed, double secondarySpeed, params AlgorithmType[] ids)
        {
            if (secondarySpeed > 0 && ids.Length > 0)
            {
                // TODO second uses first
                return FormatSpeedOutput(primarySpeed, ids[0]) + " + " + FormatSpeedOutput(secondarySpeed, ids[0]);
            }
            else if (ids.Length > 0)
            {
                return FormatSpeedOutput(primarySpeed, ids[0]);
            }
            return "N/A";
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
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                return ndpKey?.GetValue("Release") != null && Is45DotVersion((int) ndpKey.GetValue("Release"));
            }
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

        public static void SetNvidiaP0State()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "nvidiasetp0state.exe",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                using (var p = new Process { StartInfo = startInfo })
                {
                    p.Start();
                    p?.WaitForExit(10*1000);
                    if (p?.ExitCode != 0)
                        Logger.Info("NICEHASH", "nvidiasetp0state returned error code: " + p.ExitCode);
                    else
                        Logger.Info("NICEHASH", "nvidiasetp0state all OK");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", "nvidiasetp0state error: " + ex.Message);
            }
        }

        public static void VisitUrlLink(string urlLink)
        {
            try
            {
                using (var p = Process.Start(urlLink)) {}
            }
            catch (Exception ex)
            {
                Logger.Error("NICEHASH", "VisitLink error: " + ex.Message);
            }
        }
    }
}
