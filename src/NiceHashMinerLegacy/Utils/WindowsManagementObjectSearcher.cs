using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Devices.Querying.Nvidia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Utils
{
    static class WindowsManagementObjectSearcher
    {
        #region System Memory
        public static ulong TotalVisibleMemorySize { get; private set; }
        public static ulong TotalVirtualMemorySize { get; private set; }
        // PageFileSize might be redundant
        public static ulong PageFileSize => TotalVirtualMemorySize - TotalVisibleMemorySize;
        public static ulong FreePhysicalMemory { get; private set; }
        public static ulong FreeVirtualMemory { get; private set; }

        public static void QueryWin32_OperatingSystemData()
        {
            var attributes = new List<string> { "FreePhysicalMemory", "FreeVirtualMemory", "TotalVisibleMemorySize", "TotalVirtualMemorySize" };
            var attributesParams = string.Join(",", attributes);
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT {attributesParams} FROM Win32_OperatingSystem"))
            using (var query = searcher.Get())
            {
                foreach (var item in query)
                {
                    if (!(item is ManagementObject mo)) continue;
                    TotalVisibleMemorySize = Convert.ToUInt64(mo.GetPropertyValue("TotalVisibleMemorySize"));
                    TotalVirtualMemorySize = Convert.ToUInt64(mo.GetPropertyValue("TotalVirtualMemorySize"));
                    FreePhysicalMemory = Convert.ToUInt64(mo.GetPropertyValue("FreePhysicalMemory"));
                    FreeVirtualMemory = Convert.ToUInt64(mo.GetPropertyValue("FreeVirtualMemory"));
                }
            }
        }
        #endregion System Memory


        #region Video Controllers and Drivers
        public static NvidiaSmiDriver NvidiaDriver { get; private set; } = new NvidiaSmiDriver(-1, -1);
        //public static NvidiaSmiDriver NvidiaDriver { get; private set; } = new NvidiaSmiDriver(-1, -1);
        public static IReadOnlyList<VideoControllerData> AvailableVideoControllers { get; private set; }
        public static IEnumerable<VideoControllerData> BadVideoControllers {
            get
            {
                return AvailableVideoControllers?.Where(vc => vc.Status.ToLower() != "ok") ?? Enumerable.Empty<VideoControllerData>();  
            }
        }
        public static bool HasNvidiaVideoController => AvailableVideoControllers?.Any(vctrl => vctrl.IsNvidia) ?? false;

        public static void QueryWin32_VideoController()
        {

            var attributes = new List<string> { "AdapterRAM", "Name", "Description", "PNPDeviceID", "DriverVersion", "Status", "InfSection" };
            var attributesParams = string.Join(",", attributes);

            var vidControllers = new List<VideoControllerData>();
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT {attributesParams} FROM Win32_VideoController WHERE PNPDeviceID LIKE 'PCI%'"))
            using (var query = searcher.Get())
            {
                foreach (var item in query)
                {
                    if (!(item is ManagementObject mo)) continue;

                    var memTmp = Convert.ToUInt64(mo.GetPropertyValue("AdapterRAM"));
                    var vidController = new VideoControllerData
                    (
                        mo.GetPropertyValue("Name")?.ToString() ?? "key is null",
                        mo.GetPropertyValue("Description")?.ToString() ?? "key is null",
                        mo.GetPropertyValue("PNPDeviceID")?.ToString() ?? "key is null",
                        mo.GetPropertyValue("DriverVersion")?.ToString() ?? "key is null",
                        mo.GetPropertyValue("Status")?.ToString() ?? "key is null",
                        mo.GetPropertyValue("InfSection")?.ToString() ?? "key is null",
                        memTmp
                    );
                    vidControllers.Add(vidController);
                }
            }

            AvailableVideoControllers = vidControllers;

            var nvidiaDriverVersion = "NO NVIDIA DEVICES";
            // check NVIDIA drivers, we assume all NVIDIA devices are using the same driver version
            var nvidiaVideoControllerData = vidControllers.Where(vidC => vidC.IsNvidia).FirstOrDefault();
            if (nvidiaVideoControllerData != null)
            {
                NvidiaDriver = ParseNvSmiDriver(nvidiaVideoControllerData.DriverVersion);
                nvidiaDriverVersion = NvidiaDriver.ToString();
            }

            ////////////////////////////////////////////////////
            // TODO move logging outside
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"NVIDIA driver version: {nvidiaDriverVersion}");
            stringBuilder.AppendLine("QueryVideoControllers: ");
            foreach (var vidController in vidControllers)
            {
                stringBuilder.AppendLine("\tWin32_VideoController detected:");
                stringBuilder.AppendLine($"{vidController.GetFormattedString()}");
            }
            Helpers.ConsolePrint("SystemSpecs", stringBuilder.ToString());
        }

        private static NvidiaSmiDriver ParseNvSmiDriver(string windowsDriverVersion)
        {
            var winVerArray = windowsDriverVersion.Split('.');
            //we must parse windows driver format (ie. 25.21.14.1634) into nvidia driver format (ie. 416.34)
            //nvidia format driver is inside last two elements of splited windows driver string (ie. 14 + 1634)
            if (winVerArray.Length >= 2)
            {
                var firstPartOfVersion = winVerArray[winVerArray.Length - 2];
                var secondPartOfVersion = winVerArray[winVerArray.Length - 1];
                var shortVerArray = firstPartOfVersion + secondPartOfVersion;
                var driverFull = shortVerArray.Remove(0, 1).Insert(3, ".").Split('.'); // we transform that string into "nvidia" version (ie. 416.83)
                var driver = new NvidiaSmiDriver(Convert.ToInt32(driverFull[0]), Convert.ToInt32(driverFull[1])); //we create driver object from string version

                return driver;
            }
            return new NvidiaSmiDriver(-1, -1);
        }

        #endregion Video Controllers and Drivers

        #region CPU Info

        public static int NumberOfCPUCores { get; private set; }
        public static int VirtualCoresCount { get; private set; }
        public static bool IsHypeThreadingEnabled => VirtualCoresCount > NumberOfCPUCores;

        public static void QueryCPU_Info()
        {
            VirtualCoresCount = GetVirtualCoresCount();
            NumberOfCPUCores = GetNumberOfCores();
        }

        private static int GetVirtualCoresCount()
        {
            var coreCount = 0;
            using (var query = new ManagementObjectSearcher("Select NumberOfLogicalProcessors from Win32_ComputerSystem").Get())
            {
                foreach (var item in query)
                {
                    coreCount += int.Parse(item.GetPropertyValue("NumberOfLogicalProcessors").ToString());
                }
            }
            return coreCount;
        }

        private static int GetNumberOfCores()
        {
            var coreCount = 0;
            using (var searcher = new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor"))
            using (var query = searcher.Get())
            {
                foreach (var item in query)
                {
                    coreCount += int.Parse(item.GetPropertyValue("NumberOfCores").ToString());
                }
            }
            return coreCount;
        }
        #endregion CPU Info

        public static string GetCpuID()
        {
            var serial = "N/A";
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select ProcessorID from Win32_processor"))
                using (var query = searcher.Get())
                {
                    foreach (var item in query)
                    {
                        serial = item.GetPropertyValue("ProcessorID").ToString();
                    }
                }
            }
            catch { }
            return serial;
        }

        //public static string GetMotherboardID()
        //{
        //    var serial = "";
        //    using (var query = new ManagementObjectSearcher("Select SerialNumber from Win32_BaseBoard").Get())
        //    {
        //        foreach (var item in query)
        //        {
        //            serial = item.GetPropertyValue("SerialNumber").ToString();
        //        }
        //    }
        //    return serial;
        //}

        public static bool IsWmiEnabled()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                using (var query = searcher.Get())
                {
                    Helpers.ConsolePrint("NICEHASH", "WMI service seems to be running, ManagementObjectSearcher returned success.");
                    return true;
                }
            }
            catch
            {
                Helpers.ConsolePrint("NICEHASH", "ManagementObjectSearcher not working need WMI service to be running");
            }
            return false;
        }
    }
}
