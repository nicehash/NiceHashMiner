using NHM.Common;
using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;


namespace NHM.DeviceDetection.WMI
{
    internal static class VideoControllerDetector
    {
        private const string Tag = "VideoControllerDetector";
        public static Task<List<VideoControllerData>> QueryWin32_VideoControllerTask()
        {
            return Task.Run(() =>
            {
                var vidControllers = new List<VideoControllerData>();

                var attributes = new List<string> { "AdapterRAM", "Name", "Description", "PNPDeviceID", "DriverVersion", "Status", "InfSection" };
                var attributesParams = string.Join(",", attributes);
                Logger.Info(Tag, "QueryWin32_VideoControllerTask START");
                try
                {
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
                }
                catch (Exception e)
                {
                    Logger.Error(Tag, $"QueryWin32_VideoControllerTask error: {e.Message}");
                }
                Logger.Info(Tag, "QueryWin32_VideoControllerTask END");

                return vidControllers;
            });
        }
    }
}
