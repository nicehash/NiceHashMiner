using NHM.Common;
using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.WMI
{
    internal static class DeviceBusDetector
    {
        private const string Tag = "DeviceBusDetector";
        public static Task<List<DeviceBusData>> QueryWin32_DeviceBusPCITask()
        {
            return Task.Run(() =>
            {
                var devicesBusData = new List<DeviceBusData>();

                var attributes = new List<string> { "Antecedent", "Dependent" };
                var attributesParams = string.Join(",", attributes);
                Logger.Info(Tag, "QueryWin32_DeviceBusPCITask START");
                try
                {
                    using (var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT {attributesParams} FROM Win32_DeviceBus"))
                    using (var query = searcher.Get())
                    {
                        foreach (var item in query)
                        {
                            if (!(item is ManagementObject mo)) continue;

                            var deviceBusData = new DeviceBusData
                            {
                                Antecedent = mo.GetPropertyValue("Antecedent")?.ToString() ?? "key is null",
                                Dependent = mo.GetPropertyValue("Dependent")?.ToString() ?? "key is null"
                            };
                            if (deviceBusData.Dependent.Contains("PCI")) devicesBusData.Add(deviceBusData);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(Tag, $"QueryWin32_DeviceBusPCITask error: {e.Message}");
                }
                Logger.Info(Tag, "QueryWin32_DeviceBusPCITask END");

                return devicesBusData;
            });
        }
    }
}
