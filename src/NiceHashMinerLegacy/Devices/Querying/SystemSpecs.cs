using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace NiceHashMiner.Devices.Querying
{
    public static class SystemSpecs
    {
        private const string Tag = "SystemSpecs";

        public static ulong FreePhysicalMemory { get; }
        public static ulong FreeVirtualMemory { get; }

        internal static IReadOnlyList<VideoControllerData> AvailableVideoControllers { get; private set; }

        internal static bool HasNvidiaVideoController =>
            AvailableVideoControllers.Any(vctrl => vctrl.IsNvidia);

        static SystemSpecs()
        {
            var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");

            using (var searcher = new ManagementObjectSearcher(winQuery))
            {
                foreach (var obj in searcher.Get())
                {
                    if (!(obj is ManagementObject item)) continue;

                    // We only ever use these two values, so others are deleted for cleanup
                    // If we need them later we can revert this commit
                    FreePhysicalMemory = item["FreePhysicalMemory"] as ulong? ?? FreePhysicalMemory;
                    FreeVirtualMemory = item["FreeVirtualMemory"] as ulong? ?? FreeVirtualMemory;
                }
            }

            // log
            Helpers.ConsolePrint("SystemSpecs", $"FreePhysicalMemory = {FreePhysicalMemory}");
            Helpers.ConsolePrint("SystemSpecs", $"FreeVirtualMemory = {FreeVirtualMemory}");
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

        internal static IEnumerable<VideoControllerData> QueryVideoControllers()
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
                    stringBuilder.AppendLine($"{vidController.GetFormattedString()}");

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

            if (allVideoContollersOK) return Enumerable.Empty<VideoControllerData>();

            return vidControllers.Where(vc => vc.Status.ToLower() != "ok");
        }
    }
}
