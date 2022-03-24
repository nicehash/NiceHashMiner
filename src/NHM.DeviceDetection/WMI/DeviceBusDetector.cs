using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.WMI
{
    internal static class DeviceBusDetector
    {
        private const string Tag = "DeviceBusDetector";

        public static Task<IEnumerable<DeviceBusData>> QueryWin32_DeviceBusPCITask()
        {
            DeviceBusData toDeviceBusData(ManagementBaseObject item)
            {
                if (item is not ManagementObject mo) return null;
                var antecedent = mo.GetPropertyValue("Antecedent")?.ToString() ?? "key is null";
                var dependent = mo.GetPropertyValue("Dependent")?.ToString() ?? "key is null";
                if (!dependent.Contains("PCI")) return null;
                return new DeviceBusData { Antecedent = antecedent, Dependent = dependent };
            } 
            return Task.Run(() =>
            {
                Logger.Info(Tag, "QueryWin32_DeviceBusPCITask START");
                try
                {
                    var attributes = new string[] { "Antecedent", "Dependent" };
                    var attributesParams = string.Join(",", attributes);
                    using var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT {attributesParams} FROM Win32_DeviceBus");
                    using var query = searcher.Get();
                    var devicesBusData = new List<DeviceBusData>();
                    foreach (var item in query) devicesBusData.Add(toDeviceBusData(item));
                    return devicesBusData.Where(item => item is not null);
                }
                catch (Exception e)
                {
                    Logger.Error(Tag, $"QueryWin32_DeviceBusPCITask error: {e.Message}");
                }
                Logger.Info(Tag, "QueryWin32_DeviceBusPCITask END");

                return Enumerable.Empty<DeviceBusData>();
            });
        }
    }
}
