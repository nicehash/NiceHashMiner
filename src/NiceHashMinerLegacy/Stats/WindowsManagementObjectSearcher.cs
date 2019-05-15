using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Devices.Querying.Nvidia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Stats
{
    class WindowsManagementObjectSearcher
    {

        public static void GetRamAndPageFileSize()
        {
            using (var query = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize,TotalVirtualMemorySize FROM Win32_OperatingSystem").Get())
            {
                try
                {
                    foreach (var item in query)
                    {
                        var totalRam = long.Parse(item.GetPropertyValue("TotalVisibleMemorySize").ToString()) / 1024;
                        var pageFileSize = (long.Parse(item.GetPropertyValue("TotalVirtualMemorySize").ToString()) / 1024) - totalRam;
                        Helpers.ConsolePrint("NICEHASH", "Total RAM: " + totalRam + "MB");
                        Helpers.ConsolePrint("NICEHASH", "Page File Size: " + pageFileSize + "MB");
                    }
                }
                catch { }
            }
        }

        public static Tuple<ulong, ulong> GetSystemSpecs()
        {
            var winQuery = new ObjectQuery("SELECT FreePhysicalMemory,FreeVirtualMemory FROM Win32_OperatingSystem");
            ulong FreePhysicalMemory = 0;
            ulong FreeVirtualMemory = 0;
            using (var searcher = new ManagementObjectSearcher(winQuery).Get())
            {
                foreach (var obj in searcher)
                {
                    if (!(obj is ManagementObject item)) continue;

                    FreePhysicalMemory = Convert.ToUInt64(item.GetPropertyValue("FreePhysicalMemory"));
                    FreeVirtualMemory = Convert.ToUInt64(item.GetPropertyValue("FreeVirtualMemory"));
                }
            }
            return Tuple.Create(FreePhysicalMemory, FreeVirtualMemory);
        }

        public static List<VideoControllerData> GetVideoControllersData()
        {
            var vidControllers = new List<VideoControllerData>();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("QueryVideoControllers: ");

            using (var query = new ManagementObjectSearcher("root\\CIMV2",
                "SELECT AdapterRAM,Name,Description,PNPDeviceID,DriverVersion,Status,InfSection FROM Win32_VideoController WHERE PNPDeviceID LIKE 'PCI%'").Get())
            {
                foreach (var manObj in query)
                {
                    ulong.TryParse(manObj.GetPropertyValue("AdapterRAM")?.ToString() ?? "key is null", out var memTmp);
                    var vidController = new VideoControllerData
                    (
                        manObj.GetPropertyValue("Name")?.ToString() ?? "key is null",
                        manObj.GetPropertyValue("Description")?.ToString() ?? "key is null",
                        manObj.GetPropertyValue("PNPDeviceID")?.ToString() ?? "key is null",
                        manObj.GetPropertyValue("DriverVersion")?.ToString() ?? "key is null",
                        manObj.GetPropertyValue("Status")?.ToString() ?? "key is null",
                        manObj.GetPropertyValue("InfSection")?.ToString() ?? "key is null",
                        memTmp
                    );

                    stringBuilder.AppendLine("\tWin32_VideoController detected:");
                    stringBuilder.AppendLine($"{vidController.GetFormattedString()}");

                    vidControllers.Add(vidController);
                }
            }
            Helpers.ConsolePrint("SystemSpecs", stringBuilder.ToString());

            return vidControllers;
        }

        public static int GetVirtualCoresCount()
        {
            var coreCount = 0;
            using (var query = new ManagementObjectSearcher("Select NumberOfLogicalProcessors from Win32_ComputerSystem").Get())
            {
                foreach (var item in query)
                {
                    coreCount += int.Parse(item.GetPropertyValue("NumberOfLogicalProcessors")?.ToString() ?? "value is null");
                }
            }
            return coreCount;
        }

        public static int GetNumberOfCores()
        {
            var coreCount = 0;
            using (var query = new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                foreach (var item in query)
                {
                    coreCount += int.Parse(item.GetPropertyValue("NumberOfCores")?.ToString() ?? "value is null");
                }
            }
            return coreCount;
        }

        public static string GetCpuID()
        {
            var serial = "N/A";
            try
            {
                using (var query = new ManagementObjectSearcher("Select ProcessorID from Win32_processor").Get())
                {
                    foreach (var item in query)
                    {
                        serial = item.GetPropertyValue("ProcessorID")?.ToString() ?? "value is null";
                    }
                }
            }
            catch { }
            return serial;
        }

        public static string GetMotherboardID()
        {
            var serial = "";
            using (var query = new ManagementObjectSearcher("Select SerialNumber from Win32_BaseBoard").Get())
            {
                foreach (var item in query)
                {
                    serial = item.GetPropertyValue("SerialNumber")?.ToString() ?? "value is null";
                }
            }
            return serial;
        }

        public static bool IsWmiEnabled()
        {
            try
            {
                using (new ManagementObjectSearcher("root\\CIMV2", "SELECT FreePhysicalMemory FROM Win32_OperatingSystem").Get())
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

        public static NvidiaSmiDriver GetNvSmiDriver()
        {
            List<NvidiaSmiDriver> drivers = new List<NvidiaSmiDriver>();
            using (var searcher = new ManagementObjectSearcher(new WqlObjectQuery("SELECT DriverVersion FROM Win32_VideoController")).Get())
            {
                try
                {
                    foreach (ManagementObject devicesInfo in searcher)
                    {
                        var winVerArray = devicesInfo.GetPropertyValue("DriverVersion").ToString().Split('.');
                        //we must parse windows driver format (ie. 25.21.14.1634) into nvidia driver format (ie. 416.34)
                        //nvidia format driver is inside last two elements of splited windows driver string (ie. 14 + 1634)
                        if (winVerArray.Length >= 2)
                        {
                            var firstPartOfVersion = winVerArray[winVerArray.Length - 2];
                            var secondPartOfVersion = winVerArray[winVerArray.Length - 1];
                            var shortVerArray = firstPartOfVersion + secondPartOfVersion;
                            var driverFull = shortVerArray.Remove(0, 1).Insert(3, ".").Split('.'); // we transform that string into "nvidia" version (ie. 416.83)
                            NvidiaSmiDriver driver = new NvidiaSmiDriver(Convert.ToInt32(driverFull[0]), Convert.ToInt32(driverFull[1])); //we create driver object from string version

                            if (drivers.Count == 0)
                                drivers.Add(driver);
                            else
                            {
                                foreach (var ver in drivers) //we are checking if there is other driver version on system
                                {
                                    if (ver.LeftPart != driver.LeftPart || ver.RightPart != driver.RightPart)
                                        drivers.Add(driver);
                                }
                            }
                            if (drivers.Count != 1)
                            {
                                //TODO what happens if there are more driver versions??!!
                            }
                        }
                    }
                    return drivers[0]; // TODO if we will support multiple drivers this must be changed
                }
                catch (Exception e) { }
            }
            return new NvidiaSmiDriver(-1, -1);
        }
    }
}
