using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;
using NiceHashMiner.Configs;
using static NiceHashMiner.Translations;

namespace NiceHashMiner.Devices.Querying
{
    public static class SystemSpecs
    {
        private const string Tag = "SystemSpecs";

        public static ulong FreePhysicalMemory;
        public static ulong FreeSpaceInPagingFiles;
        public static ulong FreeVirtualMemory;
        public static uint LargeSystemCache;
        public static uint MaxNumberOfProcesses;
        public static ulong MaxProcessMemorySize;

        public static uint NumberOfLicensedUsers;
        public static uint NumberOfProcesses;
        public static uint NumberOfUsers;
        public static uint OperatingSystemSKU;

        public static ulong SizeStoredInPagingFiles;

        public static uint SuiteMask;

        public static ulong TotalSwapSpaceSize;
        public static ulong TotalVirtualMemorySize;
        public static ulong TotalVisibleMemorySize;

        internal static IReadOnlyList<VideoControllerData> AvailableVideoControllers { get; private set; }

        internal static bool HasNvidiaVideoController =>
            AvailableVideoControllers.Any(vctrl => vctrl.Name.ToLower().Contains("nvidia"));

        static SystemSpecs()
        {
            QueryAndLog();
        }

        private static void QueryAndLog()
        {
            var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");

            using (var searcher = new ManagementObjectSearcher(winQuery))
            {
                foreach (var obj in searcher.Get())
                {
                    if (!(obj is ManagementObject item)) continue;

                    if (item["FreePhysicalMemory"] != null)
                        ulong.TryParse(item["FreePhysicalMemory"].ToString(), out FreePhysicalMemory);
                    if (item["FreeSpaceInPagingFiles"] != null)
                        ulong.TryParse(item["FreeSpaceInPagingFiles"].ToString(), out FreeSpaceInPagingFiles);
                    if (item["FreeVirtualMemory"] != null)
                        ulong.TryParse(item["FreeVirtualMemory"].ToString(), out FreeVirtualMemory);
                    if (item["LargeSystemCache"] != null)
                        uint.TryParse(item["LargeSystemCache"].ToString(), out LargeSystemCache);
                    if (item["MaxNumberOfProcesses"] != null)
                        uint.TryParse(item["MaxNumberOfProcesses"].ToString(), out MaxNumberOfProcesses);
                    if (item["MaxProcessMemorySize"] != null)
                        ulong.TryParse(item["MaxProcessMemorySize"].ToString(), out MaxProcessMemorySize);
                    if (item["NumberOfLicensedUsers"] != null)
                        uint.TryParse(item["NumberOfLicensedUsers"].ToString(), out NumberOfLicensedUsers);
                    if (item["NumberOfProcesses"] != null)
                        uint.TryParse(item["NumberOfProcesses"].ToString(), out NumberOfProcesses);
                    if (item["NumberOfUsers"] != null)
                        uint.TryParse(item["NumberOfUsers"].ToString(), out NumberOfUsers);
                    if (item["OperatingSystemSKU"] != null)
                        uint.TryParse(item["OperatingSystemSKU"].ToString(), out OperatingSystemSKU);
                    if (item["SizeStoredInPagingFiles"] != null)
                        ulong.TryParse(item["SizeStoredInPagingFiles"].ToString(), out SizeStoredInPagingFiles);
                    if (item["SuiteMask"] != null) uint.TryParse(item["SuiteMask"].ToString(), out SuiteMask);
                    if (item["TotalSwapSpaceSize"] != null)
                        ulong.TryParse(item["TotalSwapSpaceSize"].ToString(), out TotalSwapSpaceSize);
                    if (item["TotalVirtualMemorySize"] != null)
                        ulong.TryParse(item["TotalVirtualMemorySize"].ToString(), out TotalVirtualMemorySize);
                    if (item["TotalVisibleMemorySize"] != null)
                        ulong.TryParse(item["TotalVisibleMemorySize"].ToString(), out TotalVisibleMemorySize);
                    // log
                    Helpers.ConsolePrint("SystemSpecs", $"FreePhysicalMemory = {FreePhysicalMemory}");
                    Helpers.ConsolePrint("SystemSpecs", $"FreeSpaceInPagingFiles = {FreeSpaceInPagingFiles}");
                    Helpers.ConsolePrint("SystemSpecs", $"FreeVirtualMemory = {FreeVirtualMemory}");
                    Helpers.ConsolePrint("SystemSpecs", $"LargeSystemCache = {LargeSystemCache}");
                    Helpers.ConsolePrint("SystemSpecs", $"MaxNumberOfProcesses = {MaxNumberOfProcesses}");
                    Helpers.ConsolePrint("SystemSpecs", $"MaxProcessMemorySize = {MaxProcessMemorySize}");
                    Helpers.ConsolePrint("SystemSpecs", $"NumberOfLicensedUsers = {NumberOfLicensedUsers}");
                    Helpers.ConsolePrint("SystemSpecs", $"NumberOfProcesses = {NumberOfProcesses}");
                    Helpers.ConsolePrint("SystemSpecs", $"NumberOfUsers = {NumberOfUsers}");
                    Helpers.ConsolePrint("SystemSpecs", $"OperatingSystemSKU = {OperatingSystemSKU}");
                    Helpers.ConsolePrint("SystemSpecs", $"SizeStoredInPagingFiles = {SizeStoredInPagingFiles}");
                    Helpers.ConsolePrint("SystemSpecs", $"SuiteMask = {SuiteMask}");
                    Helpers.ConsolePrint("SystemSpecs", $"TotalSwapSpaceSize = {TotalSwapSpaceSize}");
                    Helpers.ConsolePrint("SystemSpecs", $"TotalVirtualMemorySize = {TotalVirtualMemorySize}");
                    Helpers.ConsolePrint("SystemSpecs", $"TotalVisibleMemorySize = {TotalVisibleMemorySize}");
                }
            }
        }

        private static string SafeGetProperty(ManagementBaseObject mbo, string key)
        {
            try
            {
                var o = mbo.GetPropertyValue(key);
                if (o != null)
                {
                    return o.ToString();
                }
            }
            catch { }

            return "key is null";
        }

        internal static void QueryVideoControllers(bool warningsEnabled = true)
        {
            var vidControllers = new List<VideoControllerData>();
            var allVideoContollersOK = true;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("QueryVideoControllers: ");

            using (var moc = new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_VideoController WHERE PNPDeviceID LIKE 'PCI%'").Get())
            {
                foreach (var manObj in moc)
                {
                    ulong.TryParse(SafeGetProperty(manObj, "AdapterRAM"), out var memTmp);
                    var vidController = new VideoControllerData
                    (
                        SafeGetProperty(manObj, "Name"),
                        SafeGetProperty(manObj, "Description"),
                        SafeGetProperty(manObj, "PNPDeviceID"),
                        SafeGetProperty(manObj, "DriverVersion"),
                        SafeGetProperty(manObj, "Status"),
                        SafeGetProperty(manObj, "InfSection"),
                        memTmp
                    );

                    stringBuilder.AppendLine("\tWin32_VideoController detected:");
                    stringBuilder.AppendLine($"\t\tName {vidController.GetFormattedString()}");

                    // check if controller ok
                    if (allVideoContollersOK && !vidController.Status.ToLower().Equals("ok"))
                    {
                        allVideoContollersOK = false;
                    }

                    vidControllers.Add(vidController);
                }
            }

            AvailableVideoControllers = vidControllers;

            Helpers.ConsolePrint(Tag, stringBuilder.ToString());

            if (!warningsEnabled || !ConfigManager.GeneralConfig.ShowDriverVersionWarning || 
                allVideoContollersOK) return;

            var msg = Tr("We have detected a Video Controller that is not working properly. NiceHash Miner Legacy will not be able to use this Video Controller for mining. We advise you to restart your computer, or reinstall your Video Controller drivers.");
            foreach (var vc in vidControllers)
            {
                if (!vc.Status.ToLower().Equals("ok"))
                {
                    msg += Environment.NewLine
                           + string.Format(
                               Tr("Name: {0}, Status {1}, PNPDeviceID {2}"),
                               vc.Name, vc.Status, vc.PnpDeviceID);
                }
            }
            MessageBox.Show(msg,
                Tr("Warning! Video Controller not operating correctly"),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
