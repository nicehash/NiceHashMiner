using Microsoft.Win32;
using NiceHashMiner.Configs;
using NiceHashMiner.PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Security.Principal;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner
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

        public static void ConsolePrint(string grp, string text)
        {
            // try will prevent an error if something tries to print an invalid character
            try
            {
                // Console.WriteLine does nothing on x64 while debugging with VS, so use Debug. Console.WriteLine works when run from .exe
#if DEBUG
                Debug.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + grp + "] " + text);
#endif
#if !DEBUG
            Console.WriteLine("[" +DateTime.Now.ToLongTimeString() + "] [" + grp + "] " + text);
#endif

                if (ConfigManager.GeneralConfig.LogToFile && Logger.IsInit)
                    Logger.Log.Info("[" + grp + "] " + text);
            }
            catch { }  // Not gonna recursively call here in case something is seriously wrong
        }

        public static void ConsolePrint(string grp, object obj)
        {
            ConsolePrint(grp, obj.ToString());
        }

        public static void ConsolePrint(string grp, string text, params object[] arg)
        {
            ConsolePrint(grp, string.Format(text, arg));
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

            ConsolePrint("NICEHASH", "Trying to enable/disable Windows error reporting");

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
                            ConsolePrint("NICEHASH", "Current DontShowUI value: " + val);

                            if (val == 0 && en)
                            {
                                ConsolePrint("NICEHASH", "Setting register value to 1.");
                                rk.SetValue("DontShowUI", 1);
                            }
                            else if (val == 1 && !en)
                            {
                                ConsolePrint("NICEHASH", "Setting register value to 0.");
                                rk.SetValue("DontShowUI", 0);
                            }
                        }
                        else
                        {
                            ConsolePrint("NICEHASH", "Registry key not found .. creating one..");
                            rk.CreateSubKey("DontShowUI", RegistryKeyPermissionCheck.Default);
                            ConsolePrint("NICEHASH", "Setting register value to 1..");
                            rk.SetValue("DontShowUI", en ? 1 : 0);
                        }
                    }
                    else
                        ConsolePrint("NICEHASH", "Unable to open SubKey.");
                }
            }
            catch (Exception ex)
            {
                ConsolePrint("NICEHASH", "Unable to access registry. Error: " + ex.Message);
            }
        }

        public static string FormatSpeedOutput(double speed, string separator = " ")
        {
            string ret;

            if (speed < 1000)
                ret = (speed).ToString("F3", CultureInfo.InvariantCulture) + separator;
            else if (speed < 100000)
                ret = (speed * 0.001).ToString("F3", CultureInfo.InvariantCulture) + separator + "k";
            else if (speed < 100000000)
                ret = (speed * 0.000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "M";
            else
                ret = (speed * 0.000000001).ToString("F3", CultureInfo.InvariantCulture) + separator + "G";

            return ret;
        }

        public static string FormatDualSpeedOutput(double primarySpeed, double secondarySpeed=0, AlgorithmType algo = AlgorithmType.NONE) 
        {
            string ret;
            if (secondarySpeed > 0)
            {
                ret = FormatSpeedOutput(primarySpeed, "") + "/" + FormatSpeedOutput(secondarySpeed, "") + " ";
            }
            else
            {
                ret = FormatSpeedOutput(primarySpeed);
            }

            string unit;

            switch (algo)
            {
                //case AlgorithmType.Equihash:
                case AlgorithmType.ZHash:
                case AlgorithmType.Beam:
                    unit = "Sol/s";
                    break;
                case AlgorithmType.GrinCuckaroo29:
                case AlgorithmType.GrinCuckatoo31:
                    unit = "G/s";
                    break;
                default:
                    unit = "H/s";
                    break;
            }

            return ret + unit;
        }

        public static string GetMotherboardID()
        {
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            var moc = mos.Get();
            var serial = "";
            foreach (ManagementObject mo in moc)
            {
                serial = (string) mo["SerialNumber"];
            }

            return serial;
        }

        // TODO could have multiple cpus
        public static string GetCpuID()
        {
            var id = "N/A";
            try
            {
                var mbs = new ManagementObjectSearcher("Select * From Win32_processor");
                var mbsList = mbs.Get();
                foreach (ManagementObject mo in mbsList)
                {
                    id = mo["ProcessorID"].ToString();
                }
            }
            catch { }
            return id;
        }

        public static bool WebRequestTestGoogle()
        {
            const string url = "http://www.google.com";
            try
            {
                var myRequest = System.Net.WebRequest.Create(url);
                myRequest.Timeout = Globals.FirstNetworkCheckTimeoutTimeMs;
                myRequest.GetResponse();
            }
            catch (System.Net.WebException)
            {
                return false;
            }
            return true;
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

        // IsWMI enabled
        public static bool IsWmiEnabled()
        {
            try
            {
                new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get();
                ConsolePrint("NICEHASH", "WMI service seems to be running, ManagementObjectSearcher returned success.");
                return true;
            }
            catch
            {
                ConsolePrint("NICEHASH", "ManagementObjectSearcher not working need WMI service to be running");
            }
            return false;
        }

        public static void InstallVcRedist()
        {
            var cudaDevicesDetection = new Process
            {
                StartInfo =
                {
                    FileName = @"bin\vc_redist.x64.exe",
                    Arguments = "/q /norestart",
                    UseShellExecute = false,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                }
            };

            //const int waitTime = 45 * 1000; // 45seconds
            //CudaDevicesDetection.WaitForExit(waitTime);
            cudaDevicesDetection.Start();
        }

        public static void SetDefaultEnvironmentVariables()
        {
            ConsolePrint("NICEHASH", "Setting environment variables");

            var envNameValues = new Dictionary<string, string>()
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_USE_SYNC_OBJECTS", "1"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
                //{"GPU_FORCE_64BIT_PTR", "1"}  causes problems with lots of miners
            };

            foreach (var kvp in envNameValues)
            {
                var envName = kvp.Key;
                var envValue = kvp.Value;
                // Check if all the variables is set
                if (Environment.GetEnvironmentVariable(envName) == null)
                {
                    try { Environment.SetEnvironmentVariable(envName, envValue); }
                    catch (Exception e) { ConsolePrint("NICEHASH", e.ToString()); }
                }

                // Check to make sure all the values are set correctly
                if (!Environment.GetEnvironmentVariable(envName)?.Equals(envValue) ?? false)
                {
                    try { Environment.SetEnvironmentVariable(envName, envValue); }
                    catch (Exception e) { ConsolePrint("NICEHASH", e.ToString()); }
                }
            }
        }

        public static void SetNvidiaP0State()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "nvidiasetp0state.exe",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                var p = Process.Start(psi);
                p?.WaitForExit();
                if (p?.ExitCode != 0)
                    ConsolePrint("NICEHASH", "nvidiasetp0state returned error code: " + p.ExitCode);
                else
                    ConsolePrint("NICEHASH", "nvidiasetp0state all OK");
            }
            catch (Exception ex)
            {
                ConsolePrint("NICEHASH", "nvidiasetp0state error: " + ex.Message);
            }
        }

#pragma warning disable 0618
        public static AlgorithmType DualAlgoFromAlgos(AlgorithmType primary, AlgorithmType secondary)
        {
            if (primary == AlgorithmType.DaggerHashimoto)
            {
                switch (secondary)
                {
                    case AlgorithmType.Decred:
                       return AlgorithmType.DaggerDecred;
                    case AlgorithmType.Lbry:
                        return AlgorithmType.DaggerLbry;
                    case AlgorithmType.Pascal:
                        return AlgorithmType.DaggerPascal;
                    case AlgorithmType.Sia:
                        return AlgorithmType.DaggerSia;
                    case AlgorithmType.Blake2s:
                        return AlgorithmType.DaggerBlake2s;
                    case AlgorithmType.Keccak:
                        return AlgorithmType.DaggerKeccak;
                }
            }

            return primary;
        }
#pragma warning restore 0618

    }
}
