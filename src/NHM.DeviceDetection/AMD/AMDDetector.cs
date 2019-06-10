using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Device;

namespace NHM.DeviceDetection.AMD
{
    using NHM.DeviceDetection.OpenCL;
    using NHM.DeviceDetection.OpenCL.Models;
    using NHM.DeviceDetection.WMI;
    using NHM.UUID;
    using NiceHashMinerLegacy.Common.Enums;

    internal static class AMDDetector
    {
        private const string Tag = "AMDDetector";

        public static async Task<List<AMDDevice>> TryQueryAMDDevicesAsync(List<VideoControllerData> availableVideoControllers)
        {
            var amdDevices = new List<AMDDevice>();
            Logger.Info(Tag, "TryQueryAMDDevicesAsync START");
            var openCLResult = await OpenCLDetector.TryQueryOpenCLDevicesAsync();
            Logger.Info(Tag, $"TryQueryOpenCLDevicesAsync RAW: '{openCLResult.rawOutput}'");

            var result = openCLResult.parsed;
            if (result?.Platforms?.Count > 0)
            {
                var amdPlatforms = result.Platforms.Where(platform => IsAMDPlatform(platform)).ToList();
                foreach (var platform in amdPlatforms)
                {
                    var platformNum = platform.PlatformNum;
                    if (platform.Devices.Count > 0)
                    {
                        AMDDevice.GlobalOpenCLPlatformID = platformNum;
                    }
                    foreach (var oclDev in platform.Devices)
                    {
                        var infSection = "";
                        var name = oclDev._CL_DEVICE_BOARD_NAME_AMD;
                        var codename = oclDev._CL_DEVICE_NAME;
                        var gpuRAM = oclDev._CL_DEVICE_GLOBAL_MEM_SIZE;
                        var infoToHashed = $"{oclDev.DeviceID}--{DeviceType.AMD}--{gpuRAM}--{codename}--{name}";
                        // cross ref info from vid controllers with bus id
                        var vidCtrl = availableVideoControllers?.Where(vid => vid.PCI_BUS_ID == oclDev.AMD_BUS_ID).FirstOrDefault() ?? null;
                        if (vidCtrl != null)
                        {
                            infSection = vidCtrl.InfSection;
                            infoToHashed += vidCtrl.PnpDeviceID;
                        }
                        else
                        {
                            Logger.Info(Tag, $"TryQueryAMDDevicesAsync cannot find VideoControllerData with bus ID {oclDev.AMD_BUS_ID}");
                        }
                        var uuidHEX = UUID.GetHexUUID(infoToHashed);
                        var uuid = $"AMD-{uuidHEX}";
                        
                        var bd = new BaseDevice(DeviceType.AMD, uuid, name, (int)oclDev.DeviceID);
                        var amdDevice = new AMDDevice(bd, oclDev.AMD_BUS_ID, gpuRAM, codename, infSection, platformNum);
                        amdDevices.Add(amdDevice);
                    }
                }
            }
            Logger.Info(Tag, "TryQueryAMDDevicesAsync END");

            return amdDevices;
        }


        private static bool IsAMDPlatform(OpenCLPlatform platform)
        {
            return platform.PlatformName == "AMD Accelerated Parallel Processing"
                || platform.PlatformVendor == "Advanced Micro Devices, Inc."
                || platform.PlatformName.Contains("AMD");
        }
    }
}
