using System;
using System.Diagnostics;
using System.Reflection;

namespace NHMCore
{
    public static class NHMApplication
    {
        public static Version Version => Assembly.GetEntryAssembly().GetName().Version;

        //public static string ProductVersion => FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;

        public static string ProductVersion => Version.ToString();

        public static string ProductName => FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductName;

        public static string ExecutablePath => Assembly.GetExecutingAssembly().Location;

        public static void Exit(int exitCode = 0) => Environment.Exit(exitCode);
    }
}
