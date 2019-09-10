using NHM.Common;
using System;
using System.Collections.Generic;
using System.Management;

namespace NHMCore.Utils
{
    public static class SystemSpecs
    {
        private static readonly string Tag = "SystemSpecs";

        public static ulong FreePhysicalMemory { get; private set; }
        public static ulong FreeSpaceInPagingFiles { get; private set; }
        public static ulong FreeVirtualMemory { get; private set; }
        public static uint LargeSystemCache { get; private set; }
        public static uint MaxNumberOfProcesses { get; private set; }
        public static ulong MaxProcessMemorySize { get; private set; }

        public static uint NumberOfLicensedUsers { get; private set; }
        public static uint NumberOfProcesses { get; private set; }
        public static uint NumberOfUsers { get; private set; }
        public static uint OperatingSystemSKU { get; private set; }

        public static ulong SizeStoredInPagingFiles { get; private set; }

        public static uint SuiteMask { get; private set; }

        public static ulong TotalSwapSpaceSize { get; private set; }
        public static ulong TotalVirtualMemorySize { get; private set; }
        public static ulong TotalVisibleMemorySize { get; private set; }

        // PageFileSize might be redundant
        public static ulong PageFileSize => TotalVirtualMemorySize - TotalVisibleMemorySize;

        public static void QueryWin32_OperatingSystemDataAndLog()
        {
            var attributes = new List<string> { "FreePhysicalMemory", "FreeSpaceInPagingFiles", "FreeVirtualMemory", "LargeSystemCache", "MaxNumberOfProcesses", "MaxProcessMemorySize", "NumberOfLicensedUsers", "NumberOfProcesses", "NumberOfUsers", "OperatingSystemSKU", "SizeStoredInPagingFiles", "SuiteMask", "TotalSwapSpaceSize", "TotalVirtualMemorySize", "TotalVisibleMemorySize" };
            var attributesParams = string.Join(",", attributes);
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT {attributesParams} FROM Win32_OperatingSystem"))
                using (var query = searcher.Get())
                {
                    foreach (var item in query)
                    {
                        if (!(item is ManagementObject mo)) continue;
                        FreePhysicalMemory = Convert.ToUInt64(mo.GetPropertyValue("FreePhysicalMemory"));
                        FreeSpaceInPagingFiles = Convert.ToUInt64(mo.GetPropertyValue("FreeSpaceInPagingFiles"));
                        FreeVirtualMemory = Convert.ToUInt64(mo.GetPropertyValue("FreeVirtualMemory"));
                        LargeSystemCache = Convert.ToUInt32(mo.GetPropertyValue("LargeSystemCache"));
                        MaxNumberOfProcesses = Convert.ToUInt32(mo.GetPropertyValue("MaxNumberOfProcesses"));
                        MaxProcessMemorySize = Convert.ToUInt64(mo.GetPropertyValue("MaxProcessMemorySize"));
                        NumberOfLicensedUsers = Convert.ToUInt32(mo.GetPropertyValue("NumberOfLicensedUsers"));
                        NumberOfProcesses = Convert.ToUInt32(mo.GetPropertyValue("NumberOfProcesses"));
                        NumberOfUsers = Convert.ToUInt32(mo.GetPropertyValue("NumberOfUsers"));
                        OperatingSystemSKU = Convert.ToUInt32(mo.GetPropertyValue("OperatingSystemSKU"));
                        SizeStoredInPagingFiles = Convert.ToUInt64(mo.GetPropertyValue("SizeStoredInPagingFiles"));
                        SuiteMask = Convert.ToUInt32(mo.GetPropertyValue("SuiteMask"));
                        TotalSwapSpaceSize = Convert.ToUInt64(mo.GetPropertyValue("TotalSwapSpaceSize"));
                        TotalVirtualMemorySize = Convert.ToUInt64(mo.GetPropertyValue("TotalVirtualMemorySize"));
                        TotalVisibleMemorySize = Convert.ToUInt64(mo.GetPropertyValue("TotalVisibleMemorySize"));
                    }
                }
            }
            catch (Exception e)
            {
                // TODO log
                Logger.Info(Tag, $"QueryWin32_OperatingSystemDataAndLog error: {e.Message}");
            }
            Logger.Info(Tag, $"FreePhysicalMemory = {FreePhysicalMemory}, {FreePhysicalMemory / 1024} MB");
            Logger.Info(Tag, $"FreeSpaceInPagingFiles = {FreeSpaceInPagingFiles}, {FreeSpaceInPagingFiles / 1024} MB");
            Logger.Info(Tag, $"FreeVirtualMemory = {FreeVirtualMemory}, {FreeVirtualMemory / 1024} MB");
            Logger.Info(Tag, $"LargeSystemCache = {LargeSystemCache}, {LargeSystemCache / 1024} MB");
            Logger.Info(Tag, $"MaxNumberOfProcesses = {MaxNumberOfProcesses}, {MaxNumberOfProcesses / 1024} MB");
            Logger.Info(Tag, $"MaxProcessMemorySize = {MaxProcessMemorySize}, {MaxProcessMemorySize / 1024} MB");
            Logger.Info(Tag, $"NumberOfLicensedUsers = {NumberOfLicensedUsers}, {NumberOfLicensedUsers / 1024} MB");
            Logger.Info(Tag, $"NumberOfProcesses = {NumberOfProcesses}, {NumberOfProcesses / 1024} MB");
            Logger.Info(Tag, $"NumberOfUsers = {NumberOfUsers}, {NumberOfUsers / 1024} MB");
            Logger.Info(Tag, $"OperatingSystemSKU = {OperatingSystemSKU}, {OperatingSystemSKU / 1024} MB");
            Logger.Info(Tag, $"SizeStoredInPagingFiles = {SizeStoredInPagingFiles}, {SizeStoredInPagingFiles / 1024} MB");
            Logger.Info(Tag, $"SuiteMask = {SuiteMask}, {SuiteMask / 1024} MB");
            Logger.Info(Tag, $"TotalSwapSpaceSize = {TotalSwapSpaceSize}, {TotalSwapSpaceSize / 1024} MB");
            Logger.Info(Tag, $"TotalVirtualMemorySize = {TotalVirtualMemorySize}, {TotalVirtualMemorySize / 1024} MB");
            Logger.Info(Tag, $"TotalVisibleMemorySize = {TotalVisibleMemorySize}, {TotalVisibleMemorySize / 1024} MB");
            Logger.Info(Tag, $"PageFileSize = {PageFileSize}, {PageFileSize / 1024} MB");
        }

        public static bool CheckRam(int gpuCount, ulong nvRamSum, ulong amdRamSum)
        {
            // Make gpu ram needed not larger than 4GB per GPU
            var totalGpuRam = Math.Min((ulong)((nvRamSum + amdRamSum) * 0.6 / 1024),
                (ulong)gpuCount * 4 * 1024 * 1024);
            var totalSysRam = FreePhysicalMemory + FreeVirtualMemory;

            if (totalSysRam < totalGpuRam)
            {
                Logger.Info(Tag, "virtual memory size BAD");
                return false;
            }
            else
            {
                Logger.Info(Tag, "virtual memory size GOOD");
                return true;
            }
        }

        public static bool IsWmiEnabled()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                using (var query = searcher.Get())
                {
                    Logger.Info(Tag, "WMI service seems to be running, ManagementObjectSearcher returned success.");
                    return true;
                }
            }
            catch
            {
                Logger.Error(Tag, "ManagementObjectSearcher not working need WMI service to be running");
            }
            return false;
        }
    }
}
